using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using NetCoreServer;

namespace NetcoreService.ServiceHandler
{
    class ServiceHandler
    {
        public static ServiceHandler GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ServiceHandler();
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
        public HttpResponse RoutineHandler(HttpRequest request, HttpResponse response)
        {
            switch (request.Method)
            {
                case "HEAD":
                    {
                        return response.MakeHeadResponse();
                    }
                    break;
                case "GET":
                    {
                        _query.Clear();
                        string url = request.Url;
                        var queries = request.Queries();
                        foreach (var m in queries)
                        {
                            Console.WriteLine("{0}={1}", m.Key, m.Value);
                        }

                        if (getList.ContainsKey(request.Url))
                        {
                            return response.MakeGetResponse(request.Url);
                        }
                        return response.MakeGetResponse("123456");
                    }
                    break;
                case "POST":
                    {
                        return response.MakeOkResponse();
                    }
                    break;
                case "PUT":
                    {
                        return response.MakeOkResponse();
                    }
                    break;
                case "DELETE":
                    {
                        return response.MakeOkResponse();
                    }
                    break;
                case "OPTIONS":
                    {
                        return response.MakeOptionsResponse();
                    }
                    break;
                case "TRACE":
                    {
                        return response.MakeTraceResponse("trace");
                    }
                    break;
                default:
                    {
                        return response.MakeErrorResponse("Unsupported HTTP method: " + request.Method);
                    }
                    break;
            }
            return response;
        }
        private readonly ConcurrentDictionary<string, string> _query = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, Func<HttpRequest, HttpResponse>> getList = new ConcurrentDictionary<string, Func<HttpRequest, HttpResponse>>();
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private static ServiceHandler _instance;
    }
}
