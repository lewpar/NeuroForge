using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeuroForge.Shared.Network
{
    public class NetworkHelper
    {
        public static async Task<int> ReadInt32Async(SslStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte[] buffer = new byte[sizeof(int)];
            await stream.ReadAsync(buffer, 0, sizeof(int));

            return BitConverter.ToInt32(buffer);
        }

        public static async Task WriteInt32Async(SslStream stream, int value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            await stream.WriteAsync(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public static async Task<byte[]> ReadBytesAsync(SslStream stream, int length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte[] buffer = new byte[length];
            await stream.ReadAsync(buffer, 0, length);

            return buffer;
        }

        public static async Task WriteBytesAsync(SslStream stream, byte[] data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            await stream.WriteAsync(data, 0, data.Length);
        }

        public static byte[] Serialize<T>(T obj)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize<T>(obj));
        }

        public static T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data));
        }
    }
}
