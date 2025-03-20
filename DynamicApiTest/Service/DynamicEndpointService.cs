using System.Text.Json;
using System.Text.RegularExpressions;

namespace DynamicApiTest.Service
{
    public class DynamicEndpointService
    {
        private readonly Dictionary<string, DynamicEndpoint> _endpoints = new();

        public void AddEndpoint(string path, string method, string description, Func<HttpContext, Dictionary<string, string>, Task> handler, object? exampleResponse = null)
        {
            string normalizedPath = NormalizePathForSwagger(path);
            _endpoints[normalizedPath] = new DynamicEndpoint(path, method.ToUpper(), description, handler, exampleResponse);
        }

        public bool TryGetEndpoint(string requestPath, out Func<HttpContext, Task>? handler)
        {
            foreach (var endpoint in _endpoints.Values)
            {
                var match = Regex.Match(requestPath, ConvertToRegex(endpoint.Path));
                if (match.Success)
                {
                    var parameters = ExtractParameters(match);
                    handler = ctx => endpoint.Handler(ctx, parameters);
                    return true;
                }
            }

            handler = null;
            return false;
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
                            parameters = ExtractSwaggerParameters(e.Value.Path),
                            responses = new Dictionary<string, object>
                            {
                                ["200"] = new
                                {
                                    description = "Success",
                                    content = new Dictionary<string, object>
                                    {
                                        ["application/json"] = new
                                        {
                                            examples = new
                                            {
                                                defaultExample = new
                                                {
                                                    value = e.Value.ExampleResponse ?? new { message = "Example response" }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                )
            };

            return JsonSerializer.Serialize(swaggerDoc, new JsonSerializerOptions { WriteIndented = true });
        }

        private static string ConvertToRegex(string path)
        {
            return "^" + Regex.Replace(path, @"\{(\w+)\}", @"(?<$1>[^/]+)") + "$";
        }

        private static Dictionary<string, string> ExtractParameters(Match match)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var groupName in match.Groups.Keys)
            {
                if (!int.TryParse(groupName, out _))
                {
                    parameters[groupName] = match.Groups[groupName].Value;
                }
            }
            return parameters;
        }

        private static string NormalizePathForSwagger(string path)
        {
            return Regex.Replace(path, @"\{(\w+)\}", @"{$1}");
        }

        private static List<object> ExtractSwaggerParameters(string path)
        {
            var matches = Regex.Matches(path, @"\{(\w+)\}");
            return matches.Select(m => new
            {
                name = m.Groups[1].Value,
                @in = "path",
                required = true,
                schema = new { type = "string" }
            }).Cast<object>().ToList();
        }

        private class DynamicEndpoint
        {
            public string Path { get; }
            public string Method { get; }
            public string Description { get; }
            public Func<HttpContext, Dictionary<string, string>, Task> Handler { get; }
            public object? ExampleResponse { get; }

            public DynamicEndpoint(string path, string method, string description, Func<HttpContext, Dictionary<string, string>, Task> handler, object? exampleResponse)
            {
                Path = path;
                Method = method;
                Description = description;
                Handler = handler;
                ExampleResponse = exampleResponse;
            }
        }
    }
}
