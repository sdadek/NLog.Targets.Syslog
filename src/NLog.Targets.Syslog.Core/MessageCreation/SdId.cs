// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Layouts;
using NLog.Targets.Syslog.Core.Policies;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.MessageCreation
{
    internal class SdId
    {
        private readonly SimpleLayout _layout;
        private readonly SdIdPolicySet _sdIdPolicySet;

        public SdId(SimpleLayout sdIdConfig, EnforcementConfig enforcementConfig)
        {
            _layout = sdIdConfig;
            _sdIdPolicySet = new SdIdPolicySet(enforcementConfig);
        }

        public string Render(LogEventInfo logEvent)
        {
            return _layout.Render(logEvent);
        }

        public void AppendBytes(ByteArray message, string renderedSdId, EncodingSet encodings)
        {
            var sdId = _sdIdPolicySet.Apply(renderedSdId);
            var sdIdBytes = encodings.Ascii.GetBytes(sdId);
            message.Append(sdIdBytes);
        }
    }
}