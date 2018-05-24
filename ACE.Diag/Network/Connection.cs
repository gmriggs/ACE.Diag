using System.Net;
using ACE.Diag.Entity;

namespace ACE.Diag.Network
{
    /// <summary>
    /// Indicates pre-connection or fully connected
    /// </summary>
    public enum ConnectionState
    {
        Unconnected,
        Connected
    };

    /// <summary>
    /// Represents a network client connection
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// A handle to the server instance.
        /// </summary>
        public static Server Server { get => Server.Instance; }

        /// <summary>
        /// The ip address:port endpoint for this client
        /// </summary>
        public IPEndPoint Endpoint;

        /// <summary>
        /// The IP address of the client
        /// </summary>
        public string IP;

        /// <summary>
        /// The port # of the client connection
        /// </summary>
        public int Port;

        /// <summary>
        /// The ping/pong challenge
        /// </summary>
        public int Challenge;

        /// <summary>
        /// Indicates if client is in the pre-connection phase
        /// </summary>
        public ConnectionState ConnectionState;

        /// <summary>
        /// The last known GameState sent to the client
        /// </summary>
        public GameState GameState;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Connection(IPEndPoint endpoint)
        {
            IP = endpoint.Address.ToString();
            Port = endpoint.Port;
            ConnectionState = ConnectionState.Unconnected;
            Endpoint = endpoint;
        }
    }
}
