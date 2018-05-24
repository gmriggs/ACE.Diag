using System;
using System.Text;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// Serialization helper routines
    /// </summary>
    public class Serialization
    {
        /// <summary>
        /// Serializes a string to a byte array
        /// </summary>
        public static byte[] GetBytes(string str, int maxlength = 255)
        {
            if (str.Length > maxlength)
            {
                Console.WriteLine("ERROR: string length > " + maxlength + ", truncating");
                str = str.Substring(0, maxlength);
            }

            var length = (byte)str.Length;      // FIXME if bytelength > 255
            // TODO: null termination instead?
            var bytes = Encoding.ASCII.GetBytes(str);

            var data = new byte[bytes.Length + 1];
            data[0] = length;

            Buffer.BlockCopy(bytes, 0, data, 1, bytes.Length);

            return data;
        }

        /// <summary>
        /// Deserializes a string from a byte array
        /// </summary>
        /// <returns></returns>
        public static string GetString(byte[] data, int offset)
        {
            // first byte = length
            var length = data[offset];

            var strBytes = new byte[length];

            Buffer.BlockCopy(data, offset + 1, strBytes, 0, length);

            var str = Encoding.ASCII.GetString(strBytes);

            return str;
        }
    }
}
