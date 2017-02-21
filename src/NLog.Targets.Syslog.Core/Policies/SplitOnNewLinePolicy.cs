// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using NLog.Common;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class SplitOnNewLinePolicy : IBasicPolicy<string, string[]>
    {
        private readonly EnforcementConfig _enforcementConfig;
        private static readonly char[] LineSeps = { '\r', '\n' };

        public SplitOnNewLinePolicy(EnforcementConfig enforcementConfig)
        {
            _enforcementConfig = enforcementConfig;
        }

        public bool IsApplicable()
        {
            return _enforcementConfig.SplitOnNewLine;
        }

        public string[] Apply(string s)
        {
            var split = s.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            InternalLogger.Trace($"Split '{s}' on new line");
            return split;
        }
    }
}