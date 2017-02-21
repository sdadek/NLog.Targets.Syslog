// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets.Syslog.Core.Extensions;
using NLog.Targets.Syslog.Core.MessageCreation;
using NLog.Targets.Syslog.Core.MessageSend;

namespace NLog.Targets.Syslog.Core
{
    internal class LogEventMsgSet
    {
        private AsyncLogEventInfo _asyncLogEvent;
        private readonly ByteArray _buffer;
        private readonly MessageBuilder _messageBuilder;
        private readonly MessageTransmitter _messageTransmitter;
        private int _currentMessage;
        private string[] _logEntries;

        public LogEventMsgSet(AsyncLogEventInfo asyncLogEvent, ByteArray buffer, MessageBuilder messageBuilder, MessageTransmitter messageTransmitter)
        {
            _asyncLogEvent = asyncLogEvent;
            _buffer = buffer;
            _messageBuilder = messageBuilder;
            _messageTransmitter = messageTransmitter;
            _currentMessage = 0;
        }

        public LogEventMsgSet Build(Layout layout)
        {
            _logEntries = _messageBuilder.BuildLogEntries(_asyncLogEvent.LogEvent, layout);
            return this;
        }

        public Task SendAsync(CancellationToken token)
        {
            return SendAsync(token, new TaskCompletionSource<object>());
        }

        private Task SendAsync(CancellationToken token, TaskCompletionSource<object> tcs)
        {
            if (token.IsCancellationRequested)
                return tcs.CanceledTask();

            if (AllSent)
                return tcs.SucceededTask(() => _asyncLogEvent.Continuation(null));

            try
            {
                PrepareMessage();

                _messageTransmitter
                    .SendMessageAsync(_buffer, token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                            return tcs.CanceledTask();
                        if (t.Exception != null)
                        {
                            _asyncLogEvent.Continuation(t.Exception.GetBaseException());
                            tcs.SetException(t.Exception);
                            return Task.FromResult<object>(null);
                        }
                        return SendAsync(token, tcs);
                    }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                    .Unwrap();

                return tcs.Task;
            }
            catch (Exception exception)
            {
                return tcs.FailedTask(exception);
            }
        }

        private bool AllSent => _currentMessage == _logEntries.Length;

        private void PrepareMessage() => _messageBuilder.PrepareMessage(_buffer, _asyncLogEvent.LogEvent, _logEntries[_currentMessage++]);

        public override string ToString()
        {
            return _asyncLogEvent.ToFormattedMessage();
        }
    }
}