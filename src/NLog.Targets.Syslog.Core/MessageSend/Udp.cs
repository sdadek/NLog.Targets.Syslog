// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Core.Settings;

namespace NLog.Targets.Syslog.Core.MessageSend
{
    internal class Udp : MessageTransmitter
    {
        private readonly UdpClient _udp;
        private volatile bool _disposed;

        public Udp(UdpConfig udpConfig) : base(udpConfig.Server, udpConfig.Port)
        {
            _udp = new UdpClient();
        }

        public override async Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
	        if (token.IsCancellationRequested)
		        return;

			await _udp.SendAsync(message, message.Length, await GetHostAddresses(), Port);
		}

        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
			_udp.Dispose();
		}
    }
}