// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Text.RegularExpressions;
using NLog.Common;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class ReplaceComputedValuePolicy
    {
        private readonly EnforcementConfig _enforcementConfig;
        private readonly string _replaceWith;

        public ReplaceComputedValuePolicy(EnforcementConfig enforcementConfig, string replaceWith)
        {
            _enforcementConfig = enforcementConfig;
            _replaceWith = replaceWith;
        }

        public bool IsApplicable()
        {
            return _enforcementConfig.ReplaceInvalidCharacters;
        }

        public string Apply(string s, string searchFor)
        {
            if (string.IsNullOrEmpty(searchFor) || string.IsNullOrEmpty(_replaceWith) || s.Length == 0)
                return s;

            var replaced = Regex.Replace(s, searchFor, _replaceWith);
            InternalLogger.Trace($"Replaced '{searchFor}' (if found) with '{_replaceWith}' given computed value '{s}': '{replaced}'");
            return replaced;
        }
    }
}