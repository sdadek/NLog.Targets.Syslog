// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Globalization;
using NLog.Layouts;
using NLog.Targets.Syslog.Core.Policies;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.MessageCreation
{
    internal abstract class MessageBuilder
    {
        private static readonly Dictionary<RfcNumber, Func<MessageBuilderConfig, EnforcementConfig, MessageBuilder>> BuilderFactory;

        private readonly SplitOnNewLinePolicy _splitOnNewLinePolicy;
        private readonly Facility _facility;

        static MessageBuilder()
        {
            BuilderFactory = new Dictionary<RfcNumber, Func<MessageBuilderConfig, EnforcementConfig, MessageBuilder>>
            {
                { RfcNumber.Rfc3164, (msgBuilderCfg, enforcementCfg) => new Rfc3164(msgBuilderCfg.Facility, msgBuilderCfg.Rfc3164, enforcementCfg) },
                { RfcNumber.Rfc5424, (msgBuilderCfg, enforcementCfg) => new Rfc5424(msgBuilderCfg.Facility, msgBuilderCfg.Rfc5424, enforcementCfg) }
            };
        }

        public static MessageBuilder FromConfig(MessageBuilderConfig messageBuilderConfig, EnforcementConfig enforcementConfig)
        {
            return BuilderFactory[messageBuilderConfig.Rfc](messageBuilderConfig, enforcementConfig);
        }

        protected MessageBuilder(Facility facility, EnforcementConfig enforcementConfig)
        {
            _facility = facility;
            _splitOnNewLinePolicy = new SplitOnNewLinePolicy(enforcementConfig);
        }

        public string[] BuildLogEntries(LogEventInfo logEvent, Layout layout)
        {
            var originalLogEntry = layout.Render(logEvent);
            return _splitOnNewLinePolicy.IsApplicable() ? _splitOnNewLinePolicy.Apply(originalLogEntry) : new[] { originalLogEntry };
        }

        public void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string logEntry)
        {
            buffer.Reset();
            var pri = Pri(_facility, (Severity)logEvent.Level);
            PrepareMessage(buffer, logEvent, pri, logEntry);
        }

        protected abstract void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry);

        private static string Pri(Facility facility, Severity severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            return $"<{priVal.ToString(CultureInfo.InvariantCulture)}>";
        }
    }
}