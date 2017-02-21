// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Reflection;

namespace NLog.Targets.Syslog.Core.Settings
{
    internal static class UniversalAssembly
    {
        public static Assembly EntryAssembly()
        {
			return Assembly.GetEntryAssembly();
			//new StackTrace().EntryAssembly() ??
   //             Assembly.GetExecutingAssembly();
        }
    }
}