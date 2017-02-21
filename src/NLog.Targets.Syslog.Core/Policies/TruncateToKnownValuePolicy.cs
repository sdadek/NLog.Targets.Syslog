// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class TruncateToKnownValuePolicy : IBasicPolicy<string, string>
    {
        private readonly EnforcementConfig _enforcementConfig;
        private readonly int _maxLength;

        public TruncateToKnownValuePolicy(EnforcementConfig enforcementConfig, int maxLength)
        {
            _enforcementConfig = enforcementConfig;
            _maxLength = maxLength;
        }

        public bool IsApplicable()
        {
            return _enforcementConfig.TruncateFieldsToMaxLength && _maxLength > 0;
        }

        public string Apply(string s)
        {
            if (s.Length <= _maxLength)
                return s;

            var truncated = s.Substring(0, _maxLength);
            InternalLogger.Trace($"Truncated '{s}' to {_maxLength} characters: '{truncated}'");
            return truncated;
        }
    }
}