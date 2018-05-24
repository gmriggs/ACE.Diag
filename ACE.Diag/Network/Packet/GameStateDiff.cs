using System;
using System.IO;
using LZ4;
using ProtoBuf;
using ACE.Diag.Entity;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// Sends the game state diff to the network client
    /// </summary>
    public class GameStateDiff : Packet
    {
        /// <summary>
        /// The world object being added / updated / removed
        /// </summary>
        public WorldObject WorldObject;

        /// <summary>
        /// Default constructor for an added / updated / removed WorldObject
        /// </summary>
        public GameStateDiff(WorldObject wo)
        {
            Type = PacketType.GameStateDiff;
            WorldObject = wo;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="data">The GameStateDiff packet received from the server</param>
        public GameStateDiff(byte[] data)
        {
            if (data.Length < 1)
            {
                Console.WriteLine("ERROR: GameStateDiff deserialization: data.Length == " + data.Length);
                return;
            }

            Type = (PacketType)data[0];

            // get the compressed bytes from the packet
            var woCompressed = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, woCompressed, 0, woCompressed.Length);

            // decompress
            var woBytes = LZ4Codec.Unwrap(woCompressed);

            // deserialize
            var stream = new MemoryStream(woBytes);
            WorldObject = Serializer.Deserialize<WorldObject>(stream);
            stream.Close();
        }

        /// <summary>
        /// Converts the current object to GameStateDiff packet bytes
        /// </summary>
        public override byte[] Serialize()
        {
            // serialize
            var stream = new MemoryStream();
            Serializer.Serialize(stream, WorldObject);
            stream.Close();
            var woBytes = stream.ToArray();

            // compress
            var woCompressed = LZ4Codec.Wrap(woBytes);

            // build packet
            var data = new byte[1 + woCompressed.Length];
            data[0] = (byte)Type;
            Buffer.BlockCopy(woCompressed, 0, data, 1, woCompressed.Length);
            return data;
        }
    }
}
