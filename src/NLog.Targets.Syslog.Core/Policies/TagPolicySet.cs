// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class TagPolicySet : PolicySet
    {
        private const string NonAlphaNumeric = @"[^a-zA-Z0-9]";
        private const string QuestionMark = "?";
        private const int TagMaxLength = 32;

        public TagPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new ReplaceKnownValuePolicy(enforcementConfig, NonAlphaNumeric, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, TagMaxLength)
            });
        }
    }
}