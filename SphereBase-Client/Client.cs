using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SphereBase.Client
{
    public class Client
    {
        public ProtocolType ProtocolType_ = ProtocolType.Tcp;
        public uint ResponseBuffer = 1024;
        private readonly Regex ValidKeyName = new("([a-zA-Z0-9_]+)");
        int ServerPort;

        public Client(ushort serverPort = 9090)
        {
            ServerPort = serverPort;
        }

        /// <summary>
        /// Sends a Set request to the Server.
        /// </summary>
        /// <param name="key">The key you wanted to be paired with the data.</param>
        /// <param name="value">The value you wanted the key to have.</param>
        /// <returns>Returns is the server successfully managed to process a SET request.</returns>
        /// <exception cref="ArgumentException">Exception thrown when the `key` argument is invalid.</exception>
        public bool Set(string key, string value)
        {
            if (!ValidKeyName.IsMatch(key))
                throw new ArgumentException("Found invalid key name! Query aborted.");
            return SendQuery($"SET {key} TO {value}").StartsWith("OK");
        }

        /// <summary>
        /// Make a Get request with the key object.
        /// </summary>
        /// <param name="key">The key you wanted to query.</param>
        /// <returns>The data of the key you wanted to query.</returns>
        /// <exception cref="ArgumentException">Exception thrown when the `key` argument is invalid.</exception>
        public string Get(string key)
        {
            if(!ValidKeyName.IsMatch(key))
                throw new ArgumentException("Found invalid key name! Query aborted.");
            return SendQuery($"GET {key}");
        }

        /// <summary>
        /// Send a query to the server directly. (Not recommended, Using the Get(key) or Set(key, value) method is more recommended as they provide query checks.)
        /// </summary>
        /// <param name="query">The query you wanted to send.</param>
        /// <returns>The server response.</returns>
        public string SendQuery(string query)
        {
            string serverResponse = string.Empty;
            // Connects to localhost (temporary)
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, ServerPort);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType_);

            try
            {
                sender.Connect(localEndPoint);

                byte[] querySent = Encoding.UTF8.GetBytes(query + "\n");
                int byteSent = sender.Send(querySent);

                byte[] response = new byte[ResponseBuffer];

                int byteRecv = sender.Receive(response);

                serverResponse = Encoding.UTF8.GetString(response, 0, byteRecv);
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            } catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return serverResponse;
        }
    }
}
