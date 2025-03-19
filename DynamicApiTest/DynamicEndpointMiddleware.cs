//namespace DynamicApiTest
//{
//    public class DynamicEndpointMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly DynamicEndpointService _dynamicService;

//        public DynamicEndpointMiddleware(RequestDelegate next, DynamicEndpointService dynamicService)
//        {
//            _next = next;
//            _dynamicService = dynamicService;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            if (_dynamicService.TryGetEndpoint(context.Request.Path, out var handler))
//            {
//                await handler(context);
//                return;
//            }

//            await _next(context);
//        }
//    }
//}
