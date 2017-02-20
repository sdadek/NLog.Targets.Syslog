// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal abstract class MessageTransmitter
    {
        private static readonly Dictionary<ProtocolType, Func<MessageTransmitterConfig, MessageTransmitter>> TransmitterFactory;

        protected string Server { get; }

		private string _ipAddress;

        protected int Port { get; }

        static MessageTransmitter()
        {
            TransmitterFactory = new Dictionary<ProtocolType, Func<MessageTransmitterConfig, MessageTransmitter>>
            {
                { ProtocolType.Udp, messageTransmitterConfig => new Udp(messageTransmitterConfig.Udp) },
                { ProtocolType.Tcp, messageTransmitterConfig => new Tcp(messageTransmitterConfig.Tcp) }
            };
        }

        public static MessageTransmitter FromConfig(MessageTransmitterConfig messageTransmitterConfig)
        {
            return TransmitterFactory[messageTransmitterConfig.Protocol](messageTransmitterConfig);
        }

        protected MessageTransmitter(string server, int port)
        {
            Server = server;
			Port = port;
        }

        public abstract Task SendMessageAsync(ByteArray message, CancellationToken token);

        public abstract void Dispose();

		protected async Task<string> GetHostAddresses()
		{
			if (string.IsNullOrEmpty(_ipAddress))
			{
				_ipAddress = (await Dns.GetHostAddressesAsync(Server)).FirstOrDefault()?.ToString();
			}

			return _ipAddress;
		}
    }
}