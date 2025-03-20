using DynamicApiTest.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DynamicEndpointService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
var dynamicService = app.Services.GetRequiredService<DynamicEndpointService>();

// Middleware para lidar com endpoints dinâmicos
app.Use(async (context, next) =>
{
    if (dynamicService.TryGetEndpoint(context.Request.Path, out var handler))
    {
        await handler(context);
        return;
    }

    await next();
});

// Endpoint que retorna o Swagger JSON dinâmico
app.MapGet("/swagger.json", async (HttpContext context) =>
{
    var swaggerJson = dynamicService.GenerateSwaggerJson();
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(swaggerJson);
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger.json", "Dynamic API v1");
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
