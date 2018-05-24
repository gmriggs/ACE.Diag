using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// The possible responses to a network connection request
    /// </summary>
    public enum ConnectResponseType
    {
        Accepted,
        Rejected
    };

    /// <summary>
    /// Packet response to a network connection request
    /// </summary>
    public class ConnectResponse : Packet
    {
        /// <summary>
        /// The connection response packet type
        /// </summary>
        public ConnectResponseType ConnectResponseType;

        /// <summary>
        /// Supplementary reason
        /// </summary>
        public string Reason;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConnectResponse()
        {
            Type = PacketType.ConnectResponse;
        }

        /// <summary>
        /// Constructor from a connect response packet type
        /// </summary>
        public ConnectResponse(ConnectResponseType type)
        {
            Type = PacketType.ConnectResponse;
            ConnectResponseType = type;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="data">The serialized data from the network stream</param>
        public ConnectResponse(byte[] data)
        {
            base.Deserialize(data);

            if (data.Length < 2)
            {
                Console.WriteLine("ConnectResponse(byte[]) ERROR: packet size " + data.Length);
                return;
            }

            ConnectResponseType = (ConnectResponseType)data[1];

            if (data.Length > 2)
                Reason = Serialization.GetString(data, 2);
        }

        /// <summary>
        /// Converts a ConnectResponse to a byte array
        /// </summary>
        public override byte[] Serialize()
        {
            var packetType = (byte)Type;
            var connectResponseType = (byte)ConnectResponseType;

            if (Reason == null)
            {
                var data = new byte[2];
                data[0] = packetType;
                data[1] = connectResponseType;
                return data;
            }
            else
            {
                var reasonBytes = Serialization.GetBytes(Reason);
                var data = new byte[2 + reasonBytes.Length];
                data[0] = packetType;
                data[1] = connectResponseType;
                Buffer.BlockCopy(reasonBytes, 0, data, 2, reasonBytes.Length);
                return data;
            }
        }
    }
}
