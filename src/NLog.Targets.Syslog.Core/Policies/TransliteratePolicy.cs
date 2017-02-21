// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class TransliteratePolicy : IBasicPolicy<string, string>
    {
        private readonly EnforcementConfig _enforcementConfig;

        public TransliteratePolicy(EnforcementConfig enforcementConfig)
        {
			_enforcementConfig = enforcementConfig;
        }

        public bool IsApplicable()
        {
            return _enforcementConfig.Transliterate;
        }

        public string Apply(string s)
        {
            if (s.Length == 0)
                return s;

            var unidecoded = s.Unidecode();
            InternalLogger.Trace($"Transliterated '{s}' to '{unidecoded}'");
            return unidecoded;
        }
    }
}