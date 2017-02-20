// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.MessageSend;
using NLog.Targets.Syslog.Policies;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog
{
    internal class AsyncLogger
    {
        private readonly Layout _layout;
        private readonly Throttling _throttling;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;
        private readonly BlockingCollection<AsyncLogEventInfo> _queue;
        private readonly ByteArray _buffer;
        private readonly MessageTransmitter _messageTransmitter;

        public AsyncLogger(Layout loggingLayout, EnforcementConfig enforcementConfig, MessageBuilder messageBuilder, MessageTransmitterConfig messageTransmitterConfig)
        {
            _layout = loggingLayout;
            _cts = new CancellationTokenSource();
            _token = _cts.Token;
            _throttling = Throttling.FromConfig(enforcementConfig.Throttling);
            _queue = NewBlockingCollection();
            _buffer = new ByteArray(enforcementConfig.TruncateMessageTo);
            _messageTransmitter = MessageTransmitter.FromConfig(messageTransmitterConfig);
            Task.Factory.StartNew(() => ProcessQueueAsync(messageBuilder));
        }

        public async Task Log(AsyncLogEventInfo asyncLogEvent)
        {
            await _throttling.Apply(_queue.Count, delay => Enqueue(asyncLogEvent, delay));
        }

        private BlockingCollection<AsyncLogEventInfo> NewBlockingCollection()
        {
            var throttlingLimit = _throttling.Limit;

            return _throttling.BoundedBlockingCollectionNeeded ?
                new BlockingCollection<AsyncLogEventInfo>(throttlingLimit) :
                new BlockingCollection<AsyncLogEventInfo>();
        }

        private Task ProcessQueueAsync(MessageBuilder messageBuilder)
        {
            return ProcessQueueAsync(messageBuilder, new TaskCompletionSource<object>())
                .ContinueWith(t =>
                {
                    InternalLogger.Warn(t.Exception?.GetBaseException(), "ProcessQueueAsync faulted within try");
                    return ProcessQueueAsync(messageBuilder);
                }, _token, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current)
                .Unwrap();
        }

        private Task ProcessQueueAsync(MessageBuilder messageBuilder, TaskCompletionSource<object> tcs)
        {
            if (_token.IsCancellationRequested)
                return tcs.CanceledTask();

            try
            {
                var asyncLogEventInfo = _queue.Take(_token);
                var logEventMsgSet = new LogEventMsgSet(asyncLogEventInfo, _buffer, messageBuilder, _messageTransmitter);

                logEventMsgSet
                    .Build(_layout)
                    .SendAsync(_token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                        {
                            InternalLogger.Debug("Task canceled");
                            return tcs.CanceledTask();
                        }
                        if (t.Exception != null) // t.IsFaulted is true
                            InternalLogger.Warn(t.Exception.GetBaseException(), "Task faulted");
                        else
                            InternalLogger.Debug($"Successfully sent message '{logEventMsgSet}'");
                        return ProcessQueueAsync(messageBuilder, tcs);
                    }, _token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                    .Unwrap();

                return tcs.Task;
            }
            catch (Exception exception)
            {
                return tcs.FailedTask(exception);
            }
        }

        private void Enqueue(AsyncLogEventInfo asyncLogEventInfo, int delay)
        {
            _queue.TryAdd(asyncLogEventInfo, delay, _token);
            InternalLogger.Debug($"Enqueued '{asyncLogEventInfo.ToFormattedMessage()}'");
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _queue.Dispose();
            _buffer.Dispose();
            _messageTransmitter.Dispose();
        }
    }
}