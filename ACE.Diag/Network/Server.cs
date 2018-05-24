using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ACE.Diag.Entity;
using ACE.Diag.Network.Packet;

namespace ACE.Diag.Network
{
    /// <summary>
    /// ACE Diagnostics network server
    /// </summary>
    public class Server
    {
        /// <summary>
        /// The default ACE.Diagnostics port
        /// </summary>
        public static readonly int DefaultPort = 9080;

        /// <summary>
        /// The IP address of the diag server
        /// </summary>
        public string IP;

        /// <summary>
        /// The diag port number (0-65535)
        /// </summary>
        public int Port;

        /// <summary>
        /// The raw UDP server socket
        /// </summary>
        public UdpClient UdpServer;

        /// <summary>
        /// Reference endpoint for client connections
        /// </summary>
        public IPEndPoint Endpoint;

        /// <summary>
        /// The callback for sending and receiving data
        /// </summary>
        public AsyncCallback SentCallback = new AsyncCallback(SentData);
        public AsyncCallback ReceiveCallback = new AsyncCallback(ReceiveData);

        /// <summary>
        /// A list of clients connected to the server
        /// using the IP address as the index
        /// </summary>
        public Dictionary<string, Connection> Clients;

        /// <summary>
        /// A queue of input requests received by the server
        /// currently waiting for the next Update() frame to be processed
        /// </summary>
        public ConcurrentQueue<Tuple<byte[], IPEndPoint>> InputQueue;

        /// <summary>
        /// A handle to the server instance
        /// </summary>
        public static Server Instance;

        /// <summary>
        /// Constructs a diag server from an IP address and port #
        /// </summary>
        public Server(string ip = null, int port = -1)
        {
            if (ip == null)
            {
                ip = "127.0.0.1";     // loopback
                //ip = "0.0.0.0";         // all available local ips
            }

            if (port == -1) port = DefaultPort;

            Instance = this;

            IP = ip;
            Port = port;

            StartServer();
        }

        public void StartServer()
        {
            InputQueue = new ConcurrentQueue<Tuple<byte[], IPEndPoint>>();

            Console.WriteLine("Starting ACE.Diag server on " + IP + ":" + Port);
            var ip = IPAddress.Parse(IP);

            UdpServer = new UdpClient(Port);
            Endpoint = new IPEndPoint(ip, Port);
            UdpServer.BeginReceive(ReceiveCallback, UdpServer);

            // send some example messages
            var target = new IPEndPoint(ip, Port);
            for (var i = 0; i < 3; i++)
            {
                var message = "Message " + (i + 1);
                var messageBytes = Encoding.ASCII.GetBytes(message);
                UdpServer.Send(messageBytes, messageBytes.Length, target);
            }

            Clients = new Dictionary<string, Connection>();
        }

        public void Disconnect(IPEndPoint endpoint)
        {

        }

        /// <summary>
        /// Serializes a packet and sends it over the wire to a client
        /// </summary>
        /// <param name="packet">The packet to convert to serialized bytes</param>
        /// <param name="sender">The remote client to send the data to</param>
        public void SendData(byte[] packetData, IPEndPoint sender)
        {
            UdpServer.BeginSend(packetData, packetData.Length, sender, SentCallback, UdpServer);
        }

        /// <summary>
        /// Called when diagnostics server has received data
        /// </summary>
        public static void ReceiveData(IAsyncResult result)
        {
            var server = Server.Instance;
            var socket = result.AsyncState as UdpClient;

            // point towards whoever had sent the message
            var sender = server.Endpoint;

            // get the message payload
            byte[] data = null;
            try
            {
                data = socket.EndReceive(result, ref sender);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // client disconnected
                server.Disconnect(sender);
                return;
            }

            // queue processing
            server.InputQueue.Enqueue(new Tuple<byte[], IPEndPoint>(data, new IPEndPoint(sender.Address, sender.Port)));
            server.ProcessQueue();

            // schedule the next receive operation
            socket.BeginReceive(server.ReceiveCallback, socket);
        }

        public void ProcessQueue()
        {
            Tuple<byte[], IPEndPoint> item = null;
            while (InputQueue.TryDequeue(out item))
            {
                // process input packet
                var response = ParsePacket(item.Item1, item.Item2);
                ShowResponse(response);

                if (response != null)
                    SendData(response, item.Item2);
            }
        }

        /// <summary>
        /// Parses the payload data in a request packet
        /// </summary>
        /// <param name="data">The request payload data</param>
        /// <returns>The response packet, or null if no response</returns>
        public Packet.Packet ParsePacket(byte[] data, IPEndPoint sender)
        {
            // first byte is packet type
            var packetType = (PacketType)data[0];

            Packet.Packet packet = null;

            switch (packetType)
            {
                case PacketType.ConnectRequest:
                    Console.WriteLine("Packet received: " + packetType);
                    packet = new ConnectRequest(data, sender);
                    break;

                case PacketType.PingResponse:
                    Console.WriteLine("Packet received: " + packetType);
                    packet = new PingResponse(data, sender);
                    break;
            }

            if (packet == null) return null;

            return packet.Response;
        }

        public void ShowResponse(Packet.Packet packet)
        {
            if (packet == null) return;
            Console.WriteLine("Response type: " + packet.Type);

            switch (packet.Type)
            {
                case PacketType.ConnectResponse:
                    var connectResponse = (ConnectResponse)packet;
                    Console.WriteLine("Connect response type: " + connectResponse.ConnectResponseType);
                    Console.WriteLine("Reason: " + connectResponse.Reason);
                    break;

                case PacketType.PingRequest:
                    var pingRequest = (PingRequest)packet;
                    Console.WriteLine("Ping request: " + pingRequest.Random);
                    break;

                case PacketType.PingResponse:
                    var pingResponse = (PingResponse)packet;
                    Console.WriteLine("Ping response: " + pingResponse.Verify);
                    break;
            }
        }
        /// <summary>
        /// Called whent he server has sent data
        /// </summary>
        public static void SentData(IAsyncResult result)
        {
            var socket = result.AsyncState as UdpClient;

            // set to finished
            socket.EndSend(result);
        }

        /// <summary>
        /// Serializes a packet and sends it over the wire to a client
        /// </summary>
        /// <param name="packet">The packet to convert to serialized bytes</param>
        /// <param name="sender">The remote client to send the data to</param>
        public void SendData(Packet.Packet packet, IPEndPoint sender)
        {
            var responseBytes = packet.Serialize();

            UdpServer.BeginSend(responseBytes, responseBytes.Length, sender, SentCallback, UdpServer);
        }

        /// <summary>
        /// Finds an already established connection based on ip + port
        /// </summary>
        /// <param name="sender">The remote address the packet was sent from</param>
        /// <returns>An existing connection, or null if doesn't exist</returns>
        public Connection GetConnection(IPEndPoint sender)
        {
            var addr = sender.Address.ToString() + ":" + sender.Port;
            Clients.TryGetValue(addr, out var connection);
            if (connection == null)
                Console.WriteLine("GetConnection: couldn't find " + addr);

            return connection;
        }

        /// <summary>
        /// Sends a WorldObject update to all connected clients
        /// </summary>
        /// <param name="wo">The WorldObject being added / updated / removed</param>
        public void SendUpdate(WorldObject wo)
        {
            var gameStateDiff = new GameStateDiff(wo);
            var diffPacket = gameStateDiff.Serialize();

            foreach (var client in Clients.Values)
            {
                if (client == null || client.ConnectionState != ConnectionState.Connected) continue;

                SendData(diffPacket, client.Endpoint);
            }
        }
    }
}
