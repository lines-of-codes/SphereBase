using System.CommandLine;
using System.CommandLine.Invocation;
using SphereBase.Server;

class Server
{
    public static async Task Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand
        {
            new Option<ushort>(
                "--port", getDefaultValue: () => 9090,
                description: "Specify the port of the server. By default is 9090."
            ),
            new Option<string>(
                "--connection-type", getDefaultValue: () => "tcp",
                description: "Specify the Server connection type. It should be either \"udp\" or \"tcp\". By default is \"tcp\"."
            )
        };
        rootCommand.Description = "A SphereBase server.";
        NetworkManagerConfig config = new();
        rootCommand.Handler = CommandHandler.Create<ushort, string>((port, connectionType) =>
        {
            config.Port = port;
            switch(connectionType)
            {
                case "tcp":
                    config.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
                    break;
                case "udp":
                    config.ProtocolType = System.Net.Sockets.ProtocolType.Udp;
                    break;
                default:
                    throw new ArgumentException("The connection type is not TCP or UDP. Invalid argument.");
            }
        });
        rootCommand.Invoke(args);

        NetworkManager networkManager = new NetworkManager();
        Console.WriteLine($"Starting server on port {config.Port}...");
        await networkManager.Run(config);
    }
}