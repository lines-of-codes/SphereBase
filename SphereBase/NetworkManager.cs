using System.Net;
using System.Net.Sockets;
using Encoding = System.Text.Encoding;

namespace SphereBase.Server
{
    struct NetworkManagerConfig
    {
        /// <summary>
        /// The server port.
        /// </summary>
        public ushort Port = 9090;
        /// <summary>
        /// The server protocol type.
        /// </summary>
        public ProtocolType ProtocolType = ProtocolType.Tcp;
        /// <summary>
        /// The maximum possible waiting client.
        /// </summary>
        public int MaxPossibleWaitingClient = 10;
        /// <summary>
        /// The maximum size of how long a query could be. (In bytes)
        /// </summary>
        public int MaxQueryBuffer = 1024;
    }

    internal class NetworkManager
    {
        Database database = new();
        NetworkManagerConfig config;

        public async Task Run(NetworkManagerConfig config)
        {
            this.config = config;
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, config.Port);

            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, config.ProtocolType);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(config.MaxPossibleWaitingClient);

                while(true)
                {
                    Socket clientSocket = listener.Accept();

                    await HandleClientAsync(clientSocket);
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task HandleClientAsync(Socket clientSocket)
        {
            byte[] bytes = new byte[config.MaxQueryBuffer];
            string data = string.Empty;
            while (true)
            {
                int numByte = clientSocket.Receive(bytes);
                data += Encoding.UTF8.GetString(bytes, 0, numByte);

                if (data.IndexOf('\n') > -1) break;
            }
            clientSocket.Send(Encoding.UTF8.GetBytes(await database.EvaluateQuery(data)));
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
}
