using System;
using LZ4;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// Sends the game state to the network client
    /// </summary>
    public class GameState : Packet
    {
        /// <summary>
        /// A snapshot of the current state of the game.
        /// </summary>
        public Diag.GameState State;

        /// <summary>
        /// Default constructor gets the current gamestate
        /// </summary>
        public GameState()
        {
            Type = PacketType.GameState;
            State = new Diag.GameState(true);

            GetState();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="data">The GameState packet received from the server</param>
        public GameState(byte[] data)
        {
            if (data.Length < 1)
            {
                Console.WriteLine("ERROR: GameState deserialization: data.Length == " + data.Length);
                return;
            }

            Type = (PacketType)data[0];

            // get the compressed bytes from the packet
            State = new Diag.GameState();
            State.StateBytesCompressed = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, State.StateBytesCompressed, 0, State.StateBytesCompressed.Length);

            // decompress
            State.StateBytes = LZ4Codec.Unwrap(State.StateBytesCompressed);

            // persist to object vars
            State = State.LoadState();
        }

        /// <summary>
        /// Takes a snapshot of the current gamestate
        /// </summary>
        public void GetState()
        {
            State.TakeSnapshot();
        }

        /// <summary>
        /// Converts the current object to GameState packet bytes
        /// </summary>
        public override byte[] Serialize()
        {
            var data = new byte[1 + State.StateBytesCompressed.Length];
            data[0] = (byte)Type;
            Buffer.BlockCopy(State.StateBytesCompressed, 0, data, 1, State.StateBytesCompressed.Length);
            return data;
        }
    }
}
