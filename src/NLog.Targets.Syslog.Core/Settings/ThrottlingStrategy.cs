// Licensed under the BSD license
// See the LICENSE file in the project root for more information

namespace NLog.Targets.Syslog.Core.Settings
{
    /// <summary>The throttling strategy to be used</summary>
    public enum ThrottlingStrategy
    {
		/// <summary>
		/// None
		/// </summary>
        None,
		/// <summary>
		/// Discard on fixed timeout
		/// </summary>
		DiscardOnFixedTimeout,
		/// <summary>
		/// Discard on percentage timeout
		/// </summary>
		DiscardOnPercentageTimeout,
		/// <summary>
		/// Discard
		/// </summary>
        Discard,
		/// <summary>
		/// Defer for fixed time
		/// </summary>
        DeferForFixedTime,
		/// <summary>
		/// Defer for percentage time
		/// </summary>
        DeferForPercentageTime,
		/// <summary>
		/// Block
		/// </summary>
        Block
    }
}