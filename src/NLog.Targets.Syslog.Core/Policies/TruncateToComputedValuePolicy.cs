// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;
using NLog.Targets.Syslog.Core.Extensions;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class TruncateToComputedValuePolicy
    {
        // RFC 5426 (UDP/IPv6) with jumbograms: (2^32 - 1) - 40 - 8 = 4294967247
        private const long MaxLengthNotToBeExceeded = 4294967247;
        private readonly long _messageMaxLength;
        private readonly bool _assumeAscii;

        public TruncateToComputedValuePolicy(EnforcementConfig enforcementConfig, bool assumeAsciiEncoding)
        {
            _messageMaxLength = enforcementConfig.TruncateMessageTo > MaxLengthNotToBeExceeded ? MaxLengthNotToBeExceeded : enforcementConfig.TruncateMessageTo;
            _assumeAscii = assumeAsciiEncoding;
        }

        public bool IsApplicable()
        {
            return _messageMaxLength > 0;
        }

        public void Apply(ByteArray bytes)
        {
            var maxLength = _messageMaxLength;

            if (maxLength <= 0 || maxLength >= bytes.Length)
                return;

            var computedMaxLength = MaxLengthToAvoidCharCorruption(bytes, maxLength);
            bytes.Resize(computedMaxLength);
            InternalLogger.Trace($"Truncated byte array to {computedMaxLength} bytes (truncateMessageTo {_messageMaxLength})");
        }

        private long MaxLengthToAvoidCharCorruption(ByteArray bytes, long updatedMaxLength)
        {
            if (_assumeAscii)
                return updatedMaxLength;

            var computedMaxLength = bytes.Length;
            for (var i = bytes.Length - 1; i >= 0; i--)
            {
                if (computedMaxLength <= updatedMaxLength && i.IsIndexOfCharTerminatingByte(bytes))
                    break;
                computedMaxLength--;
            }
            return computedMaxLength;
        }
    }
}