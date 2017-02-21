// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Text.RegularExpressions;
using NLog.Common;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class ReplaceKnownValuePolicy : IBasicPolicy<string, string>
    {
        private readonly EnforcementConfig _enforcementConfig;
        private readonly string _searchFor;
        private readonly string _replaceWith;

        public ReplaceKnownValuePolicy(EnforcementConfig enforcementConfig, string searchFor, string replaceWith)
        {
            _enforcementConfig = enforcementConfig;
            _searchFor = searchFor;
            _replaceWith = replaceWith;
        }

        public bool IsApplicable()
        {
            return _enforcementConfig.ReplaceInvalidCharacters && !string.IsNullOrEmpty(_searchFor);
        }

        public string Apply(string s)
        {
            if (s.Length == 0)
                return s;

            var replaced = Regex.Replace(s, _searchFor, _replaceWith);
            InternalLogger.Trace($"Replaced '{_searchFor}' (if found) with '{_replaceWith}' given known value '{s}': '{replaced}'");
            return replaced;
        }
    }
}