// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class ProcIdPolicySet : PolicySet
    {
        private const string NilValue = "-";
        private const string NonPrintUsAscii = @"[^\u0021-\u007E]";
        private const string QuestionMark = "?";
        private const int ProcIdMaxLength = 128;

        public ProcIdPolicySet(EnforcementConfig enforcementConfig)
        {
            AddPolicies(new IBasicPolicy<string, string>[]
            {
                new TransliteratePolicy(enforcementConfig),
                new DefaultIfEmptyPolicy(NilValue),
                new ReplaceKnownValuePolicy(enforcementConfig, NonPrintUsAscii, QuestionMark),
                new TruncateToKnownValuePolicy(enforcementConfig, ProcIdMaxLength)
            });
        }
    }
}