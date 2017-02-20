// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.Policies
{
    internal class AsciiMessagePolicy
    {
        private const bool AssumeAsciiEncoding = true;
        private readonly TruncateToComputedValuePolicy _truncatePolicy;

        public AsciiMessagePolicy(EnforcementConfig enforcementConfig)
        {
            _truncatePolicy = new TruncateToComputedValuePolicy(enforcementConfig, AssumeAsciiEncoding);
        }

        public void Apply(ByteArray bytes)
        {
            if (_truncatePolicy.IsApplicable())
                _truncatePolicy.Apply(bytes);
        }
    }
}