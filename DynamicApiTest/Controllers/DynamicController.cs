using DynamicApiTest.Service;
using Microsoft.AspNetCore.Mvc;

namespace DynamicApiTest.Controllers
{
    [ApiController]
    [Route("api/dynamic")]
    public class DynamicController : ControllerBase
    {
        private readonly DynamicEndpointService _dynamicService;

        public DynamicController(DynamicEndpointService dynamicService)
        {
            _dynamicService = dynamicService;
        }

        [HttpPost("add-endpoint-get-v")]
        public IActionResult AddEndpointGetV()
        {
            var path = "/api/user/{id}";
            _dynamicService.AddEndpoint(path, "get", "Get user by ID", async (ctx, parameters) =>
            {
                string userId = parameters["id"];
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync($"{{\"userId\": \"{userId}\"}}");
            });

            return Ok($"Dynamic endpoint {path} added!");
        }

        [HttpPost("add-endpoint-get")]
        public IActionResult AddEndpointGet()
        {
            var path = "/api/hello";
            _dynamicService.AddEndpoint(path, "get", "Simple Hello Endpoint", async (ctx, parameters) =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync("{\"message\": \"Hello, World!\"}");
            });


            return Ok($"Dynamic endpoint {path} added!");
        }
    }
}
