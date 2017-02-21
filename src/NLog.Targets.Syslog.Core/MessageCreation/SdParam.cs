// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using NLog.Layouts;
using NLog.Targets.Syslog.Core.Extensions;
using NLog.Targets.Syslog.Core.Policies;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.MessageCreation
{
    internal class SdParam
    {
        private static readonly byte[] SpaceBytes = { 0x20 };
        private static readonly byte[] EqualBytes = { 0x3D };
        private static readonly byte[] QuotesBytes = { 0x22 };

        private readonly Layout _name;
        private readonly Layout _value;
        private readonly ParamNamePolicySet _paramNamePolicySet;
        private readonly ParamValuePolicySet _paramValuePolicySet;

        public SdParam(SdParamConfig sdParamConfig, EnforcementConfig enforcementConfig)
        {
            _name = sdParamConfig.Name;
            _value = sdParamConfig.Value;
            _paramNamePolicySet = new ParamNamePolicySet(enforcementConfig);
            _paramValuePolicySet = new ParamValuePolicySet(enforcementConfig);
        }

        public static void AppendBytes(ByteArray message, IEnumerable<SdParam> sdParams, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            message.Append(SpaceBytes);
            sdParams.ForEach(sdParam => sdParam.AppendBytes(message, logEvent, invalidNamesPattern, encodings));
        }

        public static string ToString(IEnumerable<SdParam> sdParams)
        {
            return sdParams.Aggregate(string.Empty, (acc, cur) => $"{acc} {cur.ToString()}");
        }

        public override string ToString()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            return $"{_name.Render(nullEvent)}=\"{_value.Render(nullEvent)}\"";
        }

        private void AppendBytes(ByteArray message, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            AppendNameBytes(message, logEvent, invalidNamesPattern, encodings);
            message.Append(EqualBytes);
            message.Append(QuotesBytes);
            AppendValueBytes(message, logEvent, encodings);
            message.Append(QuotesBytes);
        }

        private void AppendNameBytes(ByteArray message, LogEventInfo logEvent, string invalidNamesPattern, EncodingSet encodings)
        {
            var paramName = _paramNamePolicySet.Apply(_name.Render(logEvent), invalidNamesPattern);
            var nameBytes = encodings.Ascii.GetBytes(paramName);
            message.Append(nameBytes);
        }

        private void AppendValueBytes(ByteArray message, LogEventInfo logEvent, EncodingSet encodings)
        {
            var paramValue = _paramValuePolicySet.Apply(_value.Render(logEvent));
            var valueBytes = encodings.Utf8.GetBytes(paramValue);
            message.Append(valueBytes);
        }
    }
}