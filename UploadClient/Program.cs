using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UploadClient;


enum KnownParameters
{
    Server = 0,
    Port = 1,
    Path = 2,
}

class Program
{
    private static string ConfigFile =>
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
    
    static void Main(string[] args)
    {
        Dictionary<KnownParameters, string> parameters = new();

        if (!File.Exists(ConfigFile) && args[0] != "init") 
        {
            ShowInitCommand();
            Environment.Exit(-1);
        }
        else if (!File.Exists(ConfigFile) && args[0] == "init")
        {
            parameters = InitFromArgs(args);
        }
        else
        {
            string json = File.ReadAllText(ConfigFile);
            Config config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            parameters = config.ToDictionary();
        }
        
        Socket sock = new Socket(SocketType.Stream, ProtocolType.Tcp);

        while(true)
        {
            try
            {
                sock.Connect(new IPEndPoint(
                    IPAddress.Parse(parameters[KnownParameters.Server]), 
                    int.Parse(parameters[KnownParameters.Port])));

                

                Thread.Sleep(TimeSpan.FromMinutes(2));
            }
            catch (Exception ex)
            {
                Thread.Sleep(TimeSpan.FromMinutes(2));
                continue;
            }
        }
    }

    
    static Dictionary<KnownParameters, string> InitFromArgs(string[] args)
    {
        Dictionary<KnownParameters , string> parameters = new();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-"))
            {
                string[] splitted = args[i].Split('=');

                KnownParameters key = splitted[0].ToKownParams();

                parameters.Add(key, splitted[1]);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        return parameters;
    }

    static void ShowInitCommand()
    {
        string text = "init -s=<ipv4> -p=<port> -o=<path>";
        Console.WriteLine(text);
    }
}

internal struct Config
{
    public string IpAddress { get; set; }

    public ushort Port { get; set; }

    public string ObserverDir { get; set; }

    public Dictionary<KnownParameters, string> ToDictionary()
    {
        Dictionary<KnownParameters, string> keyValue = new();

        keyValue.Add(KnownParameters.Server, IpAddress);
        keyValue.Add(KnownParameters.Port, Port.ToString());
        keyValue.Add(KnownParameters.Path, ObserverDir);

        return keyValue;
    }
}

file static class LocalExtensions
{
    public static KnownParameters ToKownParams(this string self)
    {
        switch (self)
        {
            case "-s":
                return KnownParameters.Server;
                break;
            case "-p":
                return KnownParameters.Port;
                break;
            case "-o":
                return KnownParameters.Path;
                break;
            default:

                throw new ArgumentException();
                break;
        }
    }
}