// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Core.Settings
{
    /// <summary>The protocol to be used when transmitting a message</summary>
    public enum ProtocolType
    {
		/// <summary>
		/// Udp protocol
		/// </summary>
        Udp,
		/// <summary>
		/// Tcp protocol
		/// </summary>
        Tcp
    }
}