using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LZ4;
using ProtoBuf;
using ProtoBuf.Meta;
using ACE.Diag.Entity;
using ACE.Diag.Network.Serialization;

namespace ACE.Diag
{
    [ProtoContract]
    public class GameState
    {
        [ProtoMember(1)]
        public Dictionary<ObjectGuid, WorldObject> WorldObjects;

        /// <summary>
        /// The serialized state stored in uncompressed and compressed byte arrays
        /// </summary>
        public byte[] StateBytes;
        public byte[] StateBytesCompressed;

        /// <summary>
        /// Static constructor
        /// </summary>
        static GameState()
        {
            // add surrogates for custom types to protocol buffers
            RuntimeTypeModel.Default.Add(typeof(Vector3), false).SetSurrogate(typeof(Vector3Surrogate));
            RuntimeTypeModel.Default.Add(typeof(Quaternion), false).SetSurrogate(typeof(QuaternionSurrogate));
        }

        /// <summary>
        /// Empty constructor, mainly for ProtoBuf
        /// </summary>
        public GameState() { }

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameState(bool mapFields = false)
        {
            if (mapFields)
                MapFields();
        }

        /// <summary>
        /// Maps the GameState fields to the actual game state
        /// </summary>
        public void MapFields()
        {
            WorldObjects = Program.WorldObjects;
        }

        /// <summary>
        /// Serializes the current GameState object to a byte array
        /// </summary>
        public byte[] GetStateBytes()
        {
            //Console.WriteLine("Getting current game state");

            var stream = new MemoryStream();
            Serializer.Serialize(stream, this);
            stream.Close();

            return stream.ToArray();
        }

        /// <summary>
        /// Deserializes the binary game state
        /// stored in StateBytes into a GameState object
        /// Used to load the state of a network game
        /// upon player connection.
        /// </summary>
        /// <returns>The GameState object representing the serialized data stored in StateBytes</returns>
        public GameState LoadState()
        {
            GameState state = null;
            var stream = new MemoryStream(StateBytes);
            state = Serializer.Deserialize<GameState>(stream);
            stream.Close();

            state.StateBytes = StateBytes;
            state.StateBytesCompressed = StateBytesCompressed;

            return state;
        }

        /// <summary>
        /// Performs a deep copy of the game state
        /// to maintain a state history for network games
        /// </summary>
        public GameState Copy()
        {
            // we serialize/deserialize here,
            // perhaps a per-field object copy explicitly
            // defined in code would be faster here?

            var stateBytes = GetStateBytes();

            GameState state = null;
            var stream = new MemoryStream(stateBytes);
            state = Serializer.Deserialize<GameState>(stream);
            stream.Close();

            return state;
        }

        /// <summary>
        /// Populates StateBytes and StateBytesCompressed
        /// using the current GameState object
        /// </summary>
        public void TakeSnapshot()
        {
            StateBytes = GetStateBytes();
            StateBytesCompressed = LZ4Codec.Wrap(StateBytes);

            Console.WriteLine(string.Format("Game state length = {0} ({1} compressed)", StateBytes.Length, StateBytesCompressed.Length));
        }
    }
}
