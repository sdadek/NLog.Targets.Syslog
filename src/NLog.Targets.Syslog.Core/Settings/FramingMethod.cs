// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Core.Settings
{
    /// <summary>The framing method to be used when transmitting a message</summary>
    public enum FramingMethod
    {
		/// <summary>
		/// Non transparent method
		/// </summary>
        NonTransparent,
		/// <summary>
		/// Octet counting method
		/// </summary>
        OctetCounting
    }
}