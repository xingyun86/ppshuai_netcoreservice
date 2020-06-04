using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;

namespace NetcoreService.HttpServer
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

    class HttpCacheSession : HttpSession
    {
        public HttpCacheSession(NetCoreServer.HttpServer server) : base(server) {}

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
                            SendResponseAsync(Response.MakeTraceResponse(Request.Cache.Data));
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
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }

    class HttpCacheServer : NetCoreServer.HttpServer
    {
        public HttpCacheServer(IPAddress address, int port) : base(address, port) {}

        protected override TcpSession CreateSession() { return new HttpCacheSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }
}
