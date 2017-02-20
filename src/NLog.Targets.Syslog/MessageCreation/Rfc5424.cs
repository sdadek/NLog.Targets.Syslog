// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class Rfc5424 : MessageBuilder
    {
        private const string DefaultVersion = "1";
        private const string NilValue = "-";
        private const string TimestampFormat = "{0:yyyy-MM-ddTHH:mm:ss.ffffffK}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        private readonly string _version;
        private readonly Layout _hostnameLayout;
        private readonly Layout _appNameLayout;
        private readonly Layout _procIdLayout;
        private readonly Layout _msgIdLayout;
        private readonly StructuredData _structuredData;
        private readonly bool _disableBom;
        private readonly FqdnHostnamePolicySet _hostnamePolicySet;
        private readonly AppNamePolicySet _appNamePolicySet;
        private readonly ProcIdPolicySet _procIdPolicySet;
        private readonly MsgIdPolicySet _msgIdPolicySet;
        private readonly Utf8MessagePolicy _utf8MessagePolicy;

        public Rfc5424(Facility facility, Rfc5424Config rfc5424Config, EnforcementConfig enforcementConfig) : base(facility, enforcementConfig)
        {
            _version = DefaultVersion;
            _hostnameLayout = rfc5424Config.Hostname;
            _appNameLayout = rfc5424Config.AppName;
            _procIdLayout = NilValue;
            _msgIdLayout = NilValue;
            _structuredData = new StructuredData(rfc5424Config.StructuredData, enforcementConfig);
            _disableBom = rfc5424Config.DisableBom;
            _hostnamePolicySet = new FqdnHostnamePolicySet(enforcementConfig, rfc5424Config.DefaultHostname);
            _appNamePolicySet = new AppNamePolicySet(enforcementConfig, rfc5424Config.DefaultAppName);
            _procIdPolicySet = new ProcIdPolicySet(enforcementConfig);
            _msgIdPolicySet = new MsgIdPolicySet(enforcementConfig);
            _utf8MessagePolicy = new Utf8MessagePolicy(enforcementConfig);
        }

        protected override void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry)
        {
            var encodings = new EncodingSet(!_disableBom);

            AppendHeaderBytes(buffer, pri, logEvent, encodings);
            buffer.Append(SpaceBytes);
            AppendStructuredDataBytes(buffer, logEvent, encodings);
            buffer.Append(SpaceBytes);
            AppendMsgBytes(buffer, logEntry, encodings);

            _utf8MessagePolicy.Apply(buffer);
        }

        private void AppendHeaderBytes(ByteArray buffer, string pri, LogEventInfo logEvent, EncodingSet encodings)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var hostname = _hostnamePolicySet.Apply(_hostnameLayout.Render(logEvent));
            var appName = _appNamePolicySet.Apply(_appNameLayout.Render(logEvent));
            var procId = _procIdPolicySet.Apply(_procIdLayout.Render(logEvent));
            var msgId = _msgIdPolicySet.Apply(_msgIdLayout.Render(logEvent));
            var header = $"{pri}{_version} {timestamp} {hostname} {appName} {procId} {msgId}";
            var headerBytes = encodings.Ascii.GetBytes(header);
            buffer.Append(headerBytes);
        }

        private void AppendStructuredDataBytes(ByteArray buffer, LogEventInfo logEvent, EncodingSet encodings)
        {
            _structuredData.AppendBytes(buffer, logEvent, encodings);
        }

        private static void AppendMsgBytes(ByteArray buffer, string logEntry, EncodingSet encodings)
        {
            AppendPreambleBytes(buffer, encodings);
            AppendLogEntryBytes(buffer, logEntry, encodings);
        }

        private static void AppendPreambleBytes(ByteArray buffer, EncodingSet encodings)
        {
            var preambleBytes = encodings.Utf8.GetPreamble();
            buffer.Append(preambleBytes);
        }

        private static void AppendLogEntryBytes(ByteArray buffer, string logEntry, EncodingSet encodings)
        {
            var logEntryBytes = encodings.Utf8.GetBytes(logEntry);
            buffer.Append(logEntryBytes);
        }
    }
}