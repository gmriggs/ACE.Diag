using System;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using ACE.Diag.Entity;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// A response to a ping request
    /// </summary>
    public class PingResponse : Packet
    {
        /// <summary>
        /// The same information contained in the ping request
        /// </summary>
        public int Verify;

        /// <summary>
        /// Constructs a ping response from a ping request
        /// </summary>
        public PingResponse(PingRequest request)
        {
            Type = PacketType.PingResponse;

            Verify = request.Random;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        public PingResponse(byte[] data, IPEndPoint sender)
        {
            base.Deserialize(data);

            if (data.Length < 5)
            {
                Console.WriteLine("PingResponse: data length < 5");
                return;
            }

            Verify = BitConverter.ToInt32(data, 1);

            // verify existing connection
            var client = Server.GetConnection(sender);
            if (client == null)
            {
                var addr = sender.Address.ToString() + ":" + sender.Port;
                Console.WriteLine("PingResponse: couldn't find connection for " + addr);
                return;
            }

            if (client.Challenge != Verify)
            {
                Console.WriteLine(string.Format("PingResponse: mismatch between {0} and {1}", client.Challenge, Verify));
                return;
            }

            // connection verified, send full game state
            //var resp = new ConnectResponse(ConnectResponseType.Accepted);
            //resp.Reason = "ok";
            //Response = resp;
            Console.WriteLine(string.Format("Client {0} connected", sender.Address.ToString() + ":" + sender.Port));
            client.ConnectionState = ConnectionState.Connected;

            // TODO: move to GameState packet sender?
            var gameState = new GameState();
            Response = gameState;
            var wo = gameState.State.WorldObjects.Values.FirstOrDefault();

            // queue update in 5 seconds
            Timer timer = null;
            timer = new Timer(
                (object state) => { SendUpdate(wo); }
                , null  // no state required
                , TimeSpan.FromSeconds(10)   // initial delay
                , TimeSpan.FromSeconds(10));    // repeat time
        }

        /// <summary>
        /// Test sending a WorldObject update
        /// </summary>
        public void SendUpdate(WorldObject wo)
        {
            // update position
            //wo.Location.Pos = new Vector3(50, 50, 50);
            Console.WriteLine("SendUpdate - " + wo.Location.landblockId.East.Raw.ToString("X8"));
            wo.Location.landblockId = wo.Location.landblockId.East;
            Server.SendUpdate(wo);
        }

        /// <summary>
        /// Converts the ping response to a byte array
        /// </summary>
        public override byte[] Serialize()
        {
            var type = (byte)Type;

            var verifyBytes = BitConverter.GetBytes(Verify);

            var data = new byte[1 + verifyBytes.Length];
            data[0] = type;
            Buffer.BlockCopy(verifyBytes, 0, data, 1, verifyBytes.Length);

            return data;
        }
    }
}
