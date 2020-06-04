using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;

namespace NetcoreService.HttpsServer
{
    class CommonCache
    {
        public static CommonCache GetInstance()
        {
            if (_instance == null)
            {
                _instance = new CommonCache();
            }
            return _instance;
        }

        public bool GetCache(string key, out string value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void SetCache(string key, string value)
        {
            _cache[key] = value;
        }

        public bool DeleteCache(string key, out string value)
        {
            return _cache.TryRemove(key, out value);
        }

        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private static CommonCache _instance;
    }

    class HttpsCacheSession : HttpsSession
    {
        public HttpsCacheSession(NetCoreServer.HttpsServer server) : base(server) { }

        protected override void OnReceivedRequest(HttpRequest Request)
        {
            // Show HTTP request content
            Console.WriteLine(Request);

            // Handle user api service
            if (SendResponseAsync(ServiceHandler.ServiceHandler.GetInstance().RoutineHandler(Request, Response)) != true)
            {
                // Process HTTP request methods
                switch (Request.Method)
                {
                    case "HEAD":
                        {
                            SendResponseAsync(Response.MakeHeadResponse());
                        }
                        break;
                    case "GET":
                        {
                            // Get the cache value
                            string cache;
                            if (CommonCache.GetInstance().GetCache(Request.Url, out cache))
                            {
                                // Response with the cache value
                                SendResponseAsync(Response.MakeGetResponse(cache));
                            }
                            else
                            {
                                SendResponseAsync(Response.MakeErrorResponse("Required cache value was not found for the key: " + Request.Url));
                            }
                        }
                        break;
                    case "POST":
                        {
                            // Set the cache value
                            CommonCache.GetInstance().SetCache(Request.Url, Request.Body);
                            // Response with the cache value
                            SendResponseAsync(Response.MakeOkResponse());
                        }
                        break;
                    case "PUT":
                        {
                            // Set the cache value
                            CommonCache.GetInstance().SetCache(Request.Url, Request.Body);
                            // Response with the cache value
                            SendResponseAsync(Response.MakeOkResponse());
                        }
                        break;
                    case "DELETE":
                        {
                            // Delete the cache value
                            string cache;
                            if (CommonCache.GetInstance().DeleteCache(Request.Url, out cache))
                            {
                                // Response with the cache value
                                SendResponseAsync(Response.MakeGetResponse(cache));
                            }
                            else
                            {
                                SendResponseAsync(Response.MakeErrorResponse("Deleted cache value was not found for the key: " + Request.Url));
                            }
                        }
                        break;
                    case "OPTIONS":
                        {
                            SendResponseAsync(Response.MakeOptionsResponse());
                        }
                        break;
                    case "TRACE":
                        {
                            SendResponseAsync(Response.MakeTraceResponse(Request.Cache));
                        }
                        break;
                    default:
                        {
                            SendResponseAsync(Response.MakeErrorResponse("Unsupported HTTP method: " + Request.Method));
                        }
                        break;
                }         
            }
        }

        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            Console.WriteLine($"Request error: {error}");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTPS session caught an error: {error}");
        }
    }

    class HttpsCacheServer : NetCoreServer.HttpsServer
    {
        public HttpsCacheServer(SslContext context, IPAddress address, int port) : base(context, address, port) {}

        protected override SslSession CreateSession() { return new HttpsCacheSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTPS server caught an error: {error}");
        }
    }
}
