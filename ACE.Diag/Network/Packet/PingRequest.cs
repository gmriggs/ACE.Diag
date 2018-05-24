using System;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// Connection handshake process
    /// Used to prevent UDP header IP address spoofing
    /// and bandwidth amplification attacks
    /// </summary>
    public class PingRequest : Packet
    {
        /// <summary>
        /// A random 32-bit integer
        /// </summary>
        public int Random;

        /// <summary>
        /// Default constructor
        /// </summary>
        public PingRequest()
        {
            Type = PacketType.PingRequest;

            // generate a random number
            Random = Diag.Random.Next(int.MinValue, int.MaxValue - 1);
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        public PingRequest(byte[] data)
        {
            base.Deserialize(data);

            Random = BitConverter.ToInt32(data, 1);
        }

        /// <summary>
        /// Converts the ping request to a byte array
        /// </summary>
        public override byte[] Serialize()
        {
            var type = (byte)Type;

            var randomBytes = BitConverter.GetBytes(Random);

            var data = new byte[1 + randomBytes.Length];
            data[0] = type;
            Buffer.BlockCopy(randomBytes, 0, data, 1, randomBytes.Length);

            return data;
        }
    }
}
