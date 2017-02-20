// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Policies;
using System.Globalization;
using System.Text;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageCreation
{
    internal class Rfc3164 : MessageBuilder
    {
        private const string TimestampFormat = "{0:MMM} {0,11:d HH:mm:ss}";
        private static readonly byte[] SpaceBytes = { 0x20 };

        private readonly Layout _hostnameLayout;
        private readonly Layout _tagLayout;
        private readonly PlainHostnamePolicySet _hostnamePolicySet;
        private readonly TagPolicySet _tagPolicySet;
        private readonly PlainContentPolicySet _plainContentPolicySet;
        private readonly AsciiMessagePolicy _asciiMessagePolicy;

        public Rfc3164(Facility facility, Rfc3164Config rfc3164Config, EnforcementConfig enforcementConfig) : base(facility, enforcementConfig)
        {
            _hostnamePolicySet = new PlainHostnamePolicySet(enforcementConfig);
            _tagPolicySet = new TagPolicySet(enforcementConfig);
            _plainContentPolicySet = new PlainContentPolicySet(enforcementConfig);
            _asciiMessagePolicy = new AsciiMessagePolicy(enforcementConfig);

            _hostnameLayout = rfc3164Config.Hostname;
            _tagLayout = rfc3164Config.Tag;
        }

        protected override void PrepareMessage(ByteArray buffer, LogEventInfo logEvent, string pri, string logEntry)
        {
            var encoding = new ASCIIEncoding();

            AppendPriBytes(buffer, pri, encoding);
            AppendHeaderBytes(buffer, logEvent, encoding);
            buffer.Append(SpaceBytes);
            AppendMsgBytes(buffer, logEvent, logEntry, encoding);

            _asciiMessagePolicy.Apply(buffer);
        }

        private static void AppendPriBytes(ByteArray buffer, string pri, Encoding encoding)
        {
            var priBytes = encoding.GetBytes(pri);
            buffer.Append(priBytes);
        }

        private void AppendHeaderBytes(ByteArray buffer, LogEventInfo logEvent, Encoding encoding)
        {
            var timestamp = string.Format(CultureInfo.InvariantCulture, TimestampFormat, logEvent.TimeStamp);
            var host = _hostnamePolicySet.Apply(_hostnameLayout.Render(logEvent));
            var header = $"{timestamp} {host}";
            var headerBytes = encoding.GetBytes(header);
            buffer.Append(headerBytes);
        }

        private void AppendMsgBytes(ByteArray buffer, LogEventInfo logEvent, string logEntry, Encoding encoding)
        {
            AppendTagBytes(buffer, logEvent, encoding);
            AppendContentBytes(buffer, logEntry, encoding);
        }

        private void AppendTagBytes(ByteArray buffer, LogEventInfo logEvent, Encoding encoding)
        {
            var tag = _tagPolicySet.Apply(_tagLayout.Render(logEvent));
            var tagBytes = encoding.GetBytes(tag);
            buffer.Append(tagBytes);
        }

        private void AppendContentBytes(ByteArray buffer, string logEntry, Encoding encoding)
        {
            var content = _plainContentPolicySet.Apply(logEntry);
            var contentBytes = encoding.GetBytes(content);
            buffer.Append(contentBytes);
        }
    }
}