// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class PlainContentPolicySet : PolicySet
    {
        private const string NonSpaceOrPrintUsAscii = @"[^\u0020-\u007E]";
        private const string QuestionMark = "?";
        private const string NonAlphaNumericFirstChar = @"^([a-zA-Z0-9])(.*)$";
        private const string PrefixWithSpaceReplacement = " $1$2";

        public PlainContentPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new List<IBasicPolicy<string, string>>
            {
                new TransliteratePolicy(enforcementConfig),
                new ReplaceKnownValuePolicy(enforcementConfig, NonSpaceOrPrintUsAscii, QuestionMark),
                new ReplaceKnownValuePolicy(enforcementConfig, NonAlphaNumericFirstChar, PrefixWithSpaceReplacement)
            });
        }
    }
}