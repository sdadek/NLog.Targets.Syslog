// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Linq;
using NLog.Common;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.MessageCreation;
using NLog.Targets.Syslog.Settings;
using System.Threading.Tasks;

namespace NLog.Targets.Syslog
{
    /// <summary>Enables logging to a Unix-style Syslog server using NLog</summary>
    [Target("Syslog")]
    public class SyslogTarget : TargetWithLayout
    {
        private volatile bool _inited;
        private MessageBuilder _messageBuilder;
        private AsyncLogger[] _asyncLoggers;

        /// <summary>The enforcement to be applied on the Syslog message</summary>
        public EnforcementConfig Enforcement { get; set; }

        /// <summary>The settings used to create messages according to RFCs</summary>
		public MessageBuilderConfig MessageCreation { get; set; }

        /// <summary>The settings used to send messages to the Syslog server</summary>
        public MessageTransmitterConfig MessageSend { get; set; }

        /// <summary>Builds a new instance of the SyslogTarget class</summary>
        public SyslogTarget()
        {
            Enforcement = new EnforcementConfig();
            MessageCreation = new MessageBuilderConfig();
            MessageSend = new MessageTransmitterConfig();
        }

        /// <summary>Initializes the SyslogTarget</summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (_inited)
                DisposeDependencies();

            _messageBuilder = MessageBuilder.FromConfig(MessageCreation, Enforcement);
            _asyncLoggers = Enforcement.MessageProcessors.Select(i => new AsyncLogger(Layout, Enforcement, _messageBuilder, MessageSend)).ToArray();
            _inited = true;
        }

        /// <summary>Writes a single event</summary>
        /// <param name="asyncLogEvent">The NLog.AsyncLogEventInfo</param>
        /// <remarks>Write(LogEventInfo) is called only by Write(AsyncLogEventInfo/AsyncLogEventInfo[]): no need to override it</remarks>
        protected override void Write(AsyncLogEventInfo asyncLogEvent)
        {
            MergeEventProperties(asyncLogEvent.LogEvent);
            var asyncLoggerId = asyncLogEvent.LogEvent.SequenceID % Enforcement.MessageProcessors;
            Task.Run(() => _asyncLoggers[asyncLoggerId].Log(asyncLogEvent));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeDependencies();
            base.Dispose(disposing);
        }

        private void DisposeDependencies()
        {
            try
            {
                Enforcement.MessageProcessors.ForEach(i => _asyncLoggers[i].Dispose());
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, $"{GetType().Name} dispose error");
            }
        }
    }
}