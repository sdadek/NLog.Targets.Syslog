// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.Syslog.Extensions;
using NLog.Targets.Syslog.Settings;

namespace NLog.Targets.Syslog.MessageSend
{
    internal class Tcp : MessageTransmitter
    {
        private static readonly TimeSpan ZeroSecondsTimeSpan = TimeSpan.FromSeconds(0);
        private static readonly byte[] LineFeedBytes = { 0x0A };

        private volatile bool _neverConnected;
        private readonly TimeSpan _recoveryTime;
        private readonly KeepAlive _keepAlive;
        private readonly int _connectionCheckTimeout;
        private readonly bool _useTls;
        private readonly int _dataChunkSize;
        private readonly FramingMethod _framing;
        private TcpClient _tcp;
        private Stream _stream;
        private volatile bool _disposed;

        public Tcp(TcpConfig tcpConfig) : base(tcpConfig.Server, tcpConfig.Port)
        {
            _neverConnected = true;
            _recoveryTime = TimeSpan.FromMilliseconds(tcpConfig.ReconnectInterval);
            _keepAlive = new KeepAlive(tcpConfig.KeepAlive);
            _connectionCheckTimeout = tcpConfig.ConnectionCheckTimeout;
            _useTls = tcpConfig.UseTls;
            _framing = tcpConfig.Framing;
            _dataChunkSize = tcpConfig.DataChunkSize;
        }

        public override Task SendMessageAsync(ByteArray message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            if (_tcp?.Connected == true && IsSocketConnected())
                return WriteAsync(message, token);

            var delay = _neverConnected ? ZeroSecondsTimeSpan : _recoveryTime;
            _neverConnected = false;

            return Task.Delay(delay, token)
                .Then(_ => InitTcpClient(), token)
                .Unwrap()
                .Then(_ => ConnectAsync(), token)
                .Unwrap()
                .Then(_ => WriteAsync(message, token), token)
                .Unwrap();
        }

        private bool IsSocketConnected()
        {
            if (_connectionCheckTimeout <= 0)
                return true;

            return  _tcp.Client.Poll(_connectionCheckTimeout, SelectMode.SelectRead) && _tcp.Client.Available == 0;
        }

        private Task InitTcpClient()
        {
            DisposeSslStreamNotTcpClientInnerStream();
            DisposeTcpClientAndItsInnerStream();

            _tcp = new TcpClient();
            _tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
            _tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
            _tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 0));
            // Call WSAIoctl via IOControl
            _tcp.Client.IOControl(IOControlCode.KeepAliveValues, _keepAlive.ToByteArray(), null);

            return Task.FromResult<object>(null);
        }

        private async Task ConnectAsync()
        {
			var ipAddresss = await GetHostAddresses();

            await _tcp
                .ConnectAsync(ipAddresss, Port)
                .Then(async _ => _stream = await SslDecorate(_tcp), CancellationToken.None);
        }

        private async Task<Stream> SslDecorate(TcpClient tcpClient)
        {
            var tcpStream = tcpClient.GetStream();

            if (!_useTls)
                return tcpStream;

            // Do not dispose TcpClient inner stream when disposing SslStream (TcpClient disposes it)
            var sslStream = new SslStream(tcpStream, true);
			await sslStream.AuthenticateAsClientAsync(Server, null, SslProtocols.Tls12, false);
			
            return sslStream;
        }

        private Task WriteAsync(ByteArray message, CancellationToken token)
        {
            return FramingTask(message)
                .Then(_ => WriteAsync(0, message, token), token)
                .Unwrap();
        }

        private Task FramingTask(ByteArray message)
        {
            if (_framing == FramingMethod.NonTransparent)
            {
                message.Append(LineFeedBytes);
                return Task.FromResult<object>(null);
            }

            var octetCount = message.Length;
            var prefix = new ASCIIEncoding().GetBytes($"{octetCount} ");

			return _stream.WriteAsync(prefix, 0, prefix.Length);
		}

        private Task WriteAsync(int offset, ByteArray data, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult<object>(null);

            var toBeWrittenTotal = data.Length - offset;
            var isLastWrite = toBeWrittenTotal <= _dataChunkSize;
            var count = isLastWrite ? toBeWrittenTotal : _dataChunkSize;

            return _stream.WriteAsync(data, offset, count)
                .Then(task => isLastWrite ? task : WriteAsync(offset + _dataChunkSize, data, token), token)
                .Unwrap();
        }

        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            DisposeSslStreamNotTcpClientInnerStream();
            DisposeTcpClientAndItsInnerStream();
        }

        private void DisposeSslStreamNotTcpClientInnerStream()
        {
            if (_useTls)
                _stream?.Dispose();
        }

        private void DisposeTcpClientAndItsInnerStream()
        {
			_tcp?.Dispose();
        }
    }
}