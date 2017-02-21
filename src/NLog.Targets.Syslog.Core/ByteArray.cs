// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;

namespace NLog.Targets.Syslog.Core
{
    internal class ByteArray : IDisposable
    {
        private const int Zero = 0;
        private const int DefaultBufferCapacity = 65535;
        private const int MaxBufferCapacity = int.MaxValue;
        private readonly MemoryStream _memoryStream;

        public int Length => (int)_memoryStream.Length;

        public ByteArray(long initialCapacity)
        {
            var capacity = EnforceAllowedValues(initialCapacity);
            _memoryStream = new MemoryStream(capacity);
        }

        public static implicit operator byte[](ByteArray byteArray)
        {
			return GetBuffer<byte>(byteArray._memoryStream);
			//return byteArray.memoryStream.GetBuffer();
        }

        public byte this[int index]
        {
            get
            {
                if (index >= Length)
                    throw new IndexOutOfRangeException();

				return GetBuffer<byte>(_memoryStream)[index];
			}
        }

        public void Append(byte[] buffer)
        {
            if (buffer.Length == 0)
                return;

            _memoryStream.Write(buffer, 0, buffer.Length);
        }

        public void Reset()
        {
            _memoryStream.SetLength(Zero);
        }

        public void Resize(long newLength)
        {
            if (_memoryStream.Length != newLength)
                _memoryStream.SetLength(newLength);
        }

        private static int EnforceAllowedValues(long initialCapacity)
        {
            if (initialCapacity <= 0)
                return DefaultBufferCapacity;
            if (initialCapacity > MaxBufferCapacity)
                return MaxBufferCapacity;
            return (int)initialCapacity;
        }

        public void Dispose()
        {
            _memoryStream.SetLength(Zero);
            _memoryStream.Capacity = Zero;
            _memoryStream.Dispose();
        }
		private static byte[] GetBuffer<T>(MemoryStream ms)
		{
			var arraySegment = new ArraySegment<byte>();

			ms.TryGetBuffer(out arraySegment);
			return arraySegment.Array;
		}

	}
}