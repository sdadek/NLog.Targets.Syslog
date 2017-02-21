// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Core.Settings
{
    /// <summary>The Syslog protocol RFC to be followed</summary>
    public enum RfcNumber
    {
		/// <summary>
		/// Rfc 3164
		/// </summary>
        Rfc3164 = 3164,
		/// <summary>
		/// Rfc 5424
		/// </summary>
        Rfc5424 = 5424
    }
}