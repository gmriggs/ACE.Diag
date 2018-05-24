using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ACE.Diag.Network.Packet;

namespace ACE.Diag.Network
{
    /// <summary>
    /// ACE Diagnostics network client
    /// </summary>
    public class Client
    {
        /// <summary>
        /// The default client socket port
        /// ServerPort + n1
        /// </summary>
        public static readonly int DefaultPort = Server.DefaultPort + 1;

        public UdpClient UdpClient;

        public int ClientPort = DefaultPort;
        public IPEndPoint Endpoint;

        /// <summary>
        /// Flag indicates if connected to server
        /// </summary>
        public bool IsConnected = false;

        public GameState GameState;

        /// <summary>
        /// The callback after data has been sent
        /// </summary>
        public AsyncCallback SentCallback = new AsyncCallback(SentData);
        public AsyncCallback ReceiveCallback = new AsyncCallback(ReceiveData);

        /// <summary>
        /// Singleton client instance
        /// </summary>
        public static Client Instance;

        /// <summary>
        /// The game timer from the last GameStateDiff packet
        /// successfully received from the server
        /// </summary>
        public double LastUpdated;
        public bool IsUpdated = false;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public Client()
        {
            Init();
        }

        public void Init()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Construct a new client and automatically connect to server ip
        /// </summary>
        public Client(string ip)
        {
            Init();

            Connect(ip);
        }

        /// <summary>
        /// Connect to a server ip address
        /// </summary>
        /// <returns>Connection success</returns>
        public bool Connect(string ip)
        {
            Console.WriteLine("Connecting to " + ip);

            UdpClient = new UdpClient(ClientPort);
            Endpoint = new IPEndPoint(IPAddress.Parse(ip), Server.DefaultPort);

            // connect to server
            UdpClient.Connect(Endpoint);

            // send connect request
            var connectRequest = new ConnectRequest();
            var data = connectRequest.Serialize();
            UdpClient.Send(data, data.Length);

            // wait for response
            byte[] responseBytes = null;
            try
            {
                responseBytes = UdpClient.Receive(ref Endpoint);
            }
            catch (Exception)
            {
                //Console.WriteLine(e);
                Console.WriteLine("Couldn't connect to server");
                return false;
            }
            ShowResponse(responseBytes);

            // deserialize response packet
            var packet = GetPacket(responseBytes);
            if (packet.Type == PacketType.ConnectResponse)
            {
                var responsePacket = (ConnectResponse)packet;
                if (responsePacket.ConnectResponseType != ConnectResponseType.Accepted || !responsePacket.Reason.Equals("ok"))
                {
                    Console.WriteLine("Connection refused");
                    return false;
                }
            }
            else if (packet.Type == PacketType.PingRequest)
            {
                var pingRequest = (PingRequest)packet;

                // craft a ping response with matching info
                // this small handshake should hopefully prevent ip spoofing / amplification
                // before the larger packets are sent for world state
                var pingResponse = new PingResponse(pingRequest);
                var pingResponseBytes = pingResponse.Serialize();
                UdpClient.Send(pingResponseBytes, pingResponseBytes.Length);

                // wait for world data
                responseBytes = UdpClient.Receive(ref Endpoint);
                //ShowResponse(responseBytes);
                var gameState = (Packet.GameState)GetPacket(responseBytes);
                GameState = gameState.State;

                Console.WriteLine(string.Format("Connected to {0}:{1}", ip, Server.DefaultPort));
                IsConnected = true;

                // start receiving async
                UdpClient.BeginReceive(ReceiveData, UdpClient);
            }
            else
            {
                Console.WriteLine("Unknown connection response: " + packet.Type);
                return false;
            }

            return true;
        }

        public void ShowResponse(byte[] responseBytes)
        {
            var packetType = (PacketType)responseBytes[0];
            Console.WriteLine("Response packet type = " + packetType);

            switch (packetType)
            {
                case PacketType.ConnectResponse:
                    var packet = new ConnectResponse(responseBytes);
                    Console.WriteLine("Connect response type = " + packet.ConnectResponseType);
                    Console.WriteLine("Reason = " + packet.Reason);
                    break;

                case PacketType.PingRequest:
                    var pingRequest = new PingRequest(responseBytes);
                    Console.WriteLine("Ping request: " + pingRequest.Random);
                    break;

                case PacketType.GameState:
                    var gameState = new Packet.GameState(responseBytes);
                    Console.WriteLine(string.Format("Game state packet received ({0} bytes, {1} compressed)", gameState.State.StateBytes.Length, gameState.State.StateBytesCompressed.Length));
                    break;

                case PacketType.GameStateDiff:
                    var gameStateDiff = new GameStateDiff(responseBytes);
                    Console.WriteLine(string.Format("Game state diff packet received"));
                    break;
            }
        }

        /// <summary>
        /// Only handles response packets atm?
        /// </summary>
        /// <param name="responseBytes">The server packet sent as a response from a previous request</param>
        /// <returns>The parsed packet object</returns>
        public Packet.Packet GetPacket(byte[] responseBytes)
        {
            var packetType = (PacketType)responseBytes[0];

            Packet.Packet packet = null;

            switch (packetType)
            {
                case PacketType.ConnectResponse:
                    packet = new ConnectResponse(responseBytes);
                    break;
                case PacketType.PingRequest:
                    packet = new PingRequest(responseBytes);
                    break;
                case PacketType.GameState:
                    packet = new Packet.GameState(responseBytes);
                    break;
                case PacketType.GameStateDiff:
                    packet = new GameStateDiff(responseBytes);
                    break;
            }

            ShowResponse(responseBytes);

            return packet;
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">The packet to send, usually the InputState</param>
        public void SendPacket(Packet.Packet packet)
        {
            var packetBytes = packet.Serialize();
            //UdpClient.Send(packetBytes, packetBytes.Length);
            UdpClient.BeginSend(packetBytes, packetBytes.Length, SentData, UdpClient);
        }

        /// <summary>
        /// Called when data has been sent to server
        /// </summary>
        public static void SentData(IAsyncResult result)
        {
            var socket = result.AsyncState as UdpClient;

            // set to finished
            socket.EndSend(result);
        }

        /// <summary>
        /// Called when data has been received from server
        /// </summary>
        public static void ReceiveData(IAsyncResult result)
        {
            var client = Client.Instance;
            var socket = result.AsyncState as UdpClient;

            // point towards the client connection to server
            var sender = client.Endpoint;

            // get the message payload
            var data = socket.EndReceive(result, ref sender);

            // process incoming packet
            var response = client.ParsePacket(data, sender);
            //ShowResponse(response);

            //if (response != null) client.SendData(response, sender);

            // schedule the next receive operation
            socket.BeginReceive(client.ReceiveCallback, socket);
        }

        /// <summary>
        /// Parses the data sent from the server into a packet
        /// </summary>
        /// <param name="data">The request payload data</param>
        /// <returns>The response packet, or null if no response</returns>
        public Packet.Packet ParsePacket(byte[] data, IPEndPoint sender)
        {
            // first byte is packet type
            var packetType = (PacketType)data[0];

            Packet.Packet packet = null;

            //Console.WriteLine("Packet received: " + packetType);

            switch (packetType)
            {
                case PacketType.GameStateDiff:
                    var gameStateDiff = new GameStateDiff(data);
                    ProcessUpdate(gameStateDiff);
                    break;
            }

            if (packet == null) return null;

            return packet.Response;
        }

        public Stopwatch Timer;
        public double lastUpdate;
        public int NumPackets = 0;
        public int NumBytes = 0;

        /// <summary>
        /// Processes the game state diff packets received in an update frame
        /// </summary>
        public void ProcessUpdate(GameStateDiff diff)
        {
            var wo = diff.WorldObject;

            // determine if this is an addition / update / deletion
            var guid = wo.Guid;
            var worldObjects = GameState.WorldObjects;
            worldObjects.TryGetValue(guid, out var existObj);
            if (existObj == null)
            {
                // add new object
                worldObjects.Add(guid, wo);
            }
            else
            {
                // update existing object
                worldObjects[guid] = wo;
            }
            IsUpdated = true;
        }
    }
}
