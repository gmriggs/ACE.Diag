using System;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// A list of defined packet types.
    /// </summary>
    public enum PacketType
    {
        ConnectRequest,     // client
        ConnectResponse,    // server
        GameState,          // server
        GameStateDiff,      // server
        PingRequest,        // server
        PingResponse,       // client
        DisconnectRequest,  // client
        DisconnectResponse, // server
    };

    /// <summary>
    /// Base class for all the different network packet types
    /// that can be received by the server / sent by clients
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// A handle to the server instance
        /// </summary>
        public static Server Server { get => Server.Instance; }

        /// <summary>
        /// The packet type
        /// </summary>
        public PacketType Type;

        /// <summary>
        /// An optional response to send back to the client
        /// </summary>
        public Packet Response;

        /// <summary>
        /// Packs an array of booleans 
        /// </summary>
        /// <param name="bits">An array of boolean bits</param>
        /// <returns>The bools packed into an array of bytes</returns>
        public byte[] PackBytes(bool[] bits)
        {
            // pack (in this case, using the first bool as the lsb)
            var numBytes = (bits.Length + 7) / 8;   // sort of like a ceil()...
            var packedBytes = new byte[numBytes];
            int bitIdx = 0, byteIdx = 0;
            for (var i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    packedBytes[byteIdx] |= (byte)(((byte)1) << bitIdx);
                }
                bitIdx++;
                if (bitIdx == 8)
                {
                    bitIdx = 0;
                    byteIdx++;
                }
            }
            return packedBytes;
        }

        /// <summary>
        /// Converts the current packet to a byte array
        /// </summary>
        public virtual byte[] Serialize()
        {
            var data = new byte[1];
            data[0] = (byte)Type;
            return data;
        }

        /// <summary>
        /// Converts a byte array to a packet
        /// </summary>
        /// <param name="data"></param>
        public virtual void Deserialize(byte[] data)
        {
            if (data.Length < 1)
            {
                Console.WriteLine("Packet.Deserialize ERROR: packet size " + data.Length);
                return;
            }

            Type = (PacketType)data[0];
        }
    }
}
