// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Runtime.InteropServices;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    // Win32 header file: Mstcpip.h
    //
    // struct tcp_keepalive
    // {
    //     ULONG onoff;
    //     ULONG keepalivetime;
    //     ULONG keepaliveinterval;
    // };
    internal class KeepAlive
    {
        private readonly int _onOffOffset;
        private readonly int _timeOffset;
        private readonly int _intervalOffset;
        private readonly int _structSize;
        private readonly uint _onOff;
        private readonly uint _time;
        private readonly uint _interval;

        public KeepAlive(KeepAliveConfig keepAliveConfig)
        {
			var uintSize = Marshal.SizeOf<uint>();// (typeof(uint));
            _onOffOffset = 0;
            _timeOffset = uintSize;
            _intervalOffset = 2 * uintSize;
            _structSize = 3 * uintSize;
            _onOff = (uint)(keepAliveConfig.Enabled ? 1 : 0);
            _time = (uint)keepAliveConfig.Timeout;
            _interval = (uint)keepAliveConfig.Interval;
        }

        public byte[] ToByteArray()
        {
            var keepAliveSettings = new byte[_structSize];
            BitConverter.GetBytes(_onOff).CopyTo(keepAliveSettings, _onOffOffset);
            BitConverter.GetBytes(_time).CopyTo(keepAliveSettings, _timeOffset);
            BitConverter.GetBytes(_interval).CopyTo(keepAliveSettings, _intervalOffset);
            return keepAliveSettings;
        }
    }
}