// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using NLog.Common;

namespace NLog.Targets.Syslog.Core.Policies
{
    internal class DefaultIfEmptyPolicy : IBasicPolicy<string, string>
    {
        private readonly string _defaultValue;

        public DefaultIfEmptyPolicy(string defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public bool IsApplicable()
        {
            return !string.IsNullOrEmpty(_defaultValue);
        }

        public string Apply(string s)
        {
            if (s.Length != 0)
                return s;

            InternalLogger.Trace($"Applied default value '{_defaultValue}'");
            return _defaultValue;
        }
    }
}