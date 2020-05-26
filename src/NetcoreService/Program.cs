using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;

namespace NetcoreService
{
    class Program
    {
        static void Main(string[] args)
        {
            bool sslEnable = false;
            string httpOtion = sslEnable ? "https" : "http";
            // HTTPS server port or HTTP server port
            int port = sslEnable ? 8443 : 8080;
            // HTTPS server content path or HTTP server content path
            string www = "../../../../www/api";

            if (args.Length > 0)
                port = int.Parse(args[0]);

            if (args.Length > 1)
                www = args[1];

            // Create and prepare a new SSL server context and Create a new HTTPS server or Create a new HTTP server
            HttpsServer.HttpsCacheServer https_server = null; 
            HttpServer.HttpCacheServer http_server = null;
            if (sslEnable)
            {
                https_server = new HttpsServer.HttpsCacheServer(new SslContext(SslProtocols.Tls12, new X509Certificate2("server.pfx", "qwerty")), IPAddress.Any, port);

            }
            else
            {
                http_server = new HttpServer.HttpCacheServer(IPAddress.Any, port);
            }
            Console.WriteLine($"{httpOtion} server port: {port}");
            Console.WriteLine($"{httpOtion} server static content path: {www}");
            Console.WriteLine($"{httpOtion} server website: {httpOtion}://localhost:{port}/api/index.html");

            Console.WriteLine();

            if (sslEnable)
            {
                https_server.AddStaticContent(www, "/api");
            }
            else
            {
                http_server.AddStaticContent(www, "/api");
            }

            // Start the server
            Console.Write("Server starting...");

            if (sslEnable)
            {
                https_server.Start();
            }
            else
            {
                http_server.Start();
            }
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (;;)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    if (sslEnable)
                    {
                        https_server.Restart();
                    }
                    else
                    {
                        http_server.Restart();
                    }
                    Console.WriteLine("Done!");
                }
            }

            // Stop the server
            Console.Write("Server stopping...");
            if (sslEnable)
            {
                https_server.Stop();
            }
            else
            {
                http_server.Stop();
            }
            Console.WriteLine("Done!");
        }
    }
}
