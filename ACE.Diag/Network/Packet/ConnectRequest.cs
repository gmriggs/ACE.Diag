using System;
using System.Net;

namespace ACE.Diag.Network.Packet
{
    /// <summary>
    /// The packet to initiate a connection request
    /// </summary>
    public class ConnectRequest : Packet
    {
        /// <summary>
        /// Derived from the endpoint request
        /// </summary>
        public string IP;

        /// <summary>
        /// Used to a prevent a UDP amplification attack
        /// </summary>
        public string Padding = "ACE";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConnectRequest()
        {
            Type = PacketType.ConnectRequest;
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="data">The request payload data</param>
        /// <param name="sender">The remote client ip/port</param>
        public ConnectRequest(byte[] data, IPEndPoint sender)
        {
            base.Deserialize(data);

            // get the remote IP address and port
            var addr = sender.Address.ToString() + ":" + sender.Port;
            Console.WriteLine("Connection request from " + addr);

            // reserve slot for client
            // TODO: free slot if no pingback within X seconds
            var client = new Connection(sender);

            AddClient(addr, client);

            // headers can be spoofed, so to verify the ip address
            // contained in the header, we must send a challenge request
            var pingRequest = new PingRequest();
            client.Challenge = pingRequest.Random;

            Response = pingRequest;
        }

        public void AddClient(string addr, Connection client)
        {
            if (Server.Clients.ContainsKey(addr))
                Server.Clients.Remove(addr);

            Server.Clients.Add(addr, client);
        }
    }
}
