// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class AppNamePolicySet : PolicySet
    {
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int AppNameMaxLength = 48;

        public AppNamePolicySet(EnforcementConfig enforcementConfig, string defaultAppName)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new DefaultIfEmptyPolicy(defaultAppName),
                new ReplaceKnownValuePolicy(enforcementConfig, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, AppNameMaxLength)
            });
        }
    }
}