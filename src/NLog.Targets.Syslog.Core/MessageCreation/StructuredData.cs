// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using NLog.Layouts;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.MessageCreation
{
    internal class StructuredData
    {
        private const string NilValue = "-";
        private static readonly byte[] NilValueBytes = { 0x2D };

        private readonly Layout _fromEventProperties;
        private readonly IList<SdElement> _sdElements;

        public StructuredData(StructuredDataConfig sdConfig, EnforcementConfig enforcementConfig)
        {
            _fromEventProperties = sdConfig.FromEventProperties;
            _sdElements = sdConfig.SdElements.Select(sdElementConfig => new SdElement(sdElementConfig, enforcementConfig)).ToList();
        }

        public void AppendBytes(ByteArray message, LogEventInfo logEvent, EncodingSet encodings)
        {
            var sdFromEvtProps = _fromEventProperties.Render(logEvent);

            if (!string.IsNullOrEmpty(sdFromEvtProps))
            {
                var sdBytes = encodings.Utf8.GetBytes(sdFromEvtProps);
                message.Append(sdBytes);
                return;
            }

            if (_sdElements.Count == 0)
                message.Append(NilValueBytes);
            else
                SdElement.AppendBytes(message, _sdElements, logEvent, encodings);
        }

        public override string ToString()
        {
            return _sdElements.Count == 0 ? NilValue : SdElement.ToString(_sdElements);
        }
    }
}