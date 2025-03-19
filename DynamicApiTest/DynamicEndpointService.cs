using System.Collections.Concurrent;
using System.Text.Json;

namespace DynamicApiTest
{
    public class DynamicEndpointService
    {
        private readonly ConcurrentDictionary<string, (string Method, string Description)> _endpoints = new();

        public void AddEndpoint(string path, string method, string description, RequestDelegate handler)
        {
            _endpoints[path] = (method, description);
            Handlers[path] = handler;
        }

        public bool TryGetEndpoint(string path, out RequestDelegate handler)
        {
            return Handlers.TryGetValue(path, out handler);
        }

        public IEnumerable<KeyValuePair<string, (string Method, string Description)>> GetEndpoints()
        {
            return _endpoints;
        }

        public string GenerateSwaggerJson()
        {
            var swaggerDoc = new
            {
                openapi = "3.0.1",
                info = new { title = "Dynamic API", version = "v1" },
                paths = _endpoints.ToDictionary(
                    e => e.Key,
                    e => new Dictionary<string, object>
                    {
                        [e.Value.Method.ToLower()] = new
                        {
                            summary = e.Value.Description,
                            responses = new Dictionary<string, object>
                            {
                                ["200"] = new { description = "Success" }
                            }
                        }
                    }
                )
            };

            return JsonSerializer.Serialize(swaggerDoc, new JsonSerializerOptions { WriteIndented = true });
        }

        private readonly ConcurrentDictionary<string, RequestDelegate> Handlers = new();
    }
}
