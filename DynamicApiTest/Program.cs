using DynamicApiTest;

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

// Endpoint para adicionar novos endpoints dinamicamente
app.MapPost("/api/add-endpoint", async (HttpContext context) =>
{
    var path = "/api/custom-endpoint";
    dynamicService.AddEndpoint(path, "get", "Dynamic endpoint test", async ctx =>
    {
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync("{\"message\": \"This is a dynamic endpoint!\"}");
    });

    await context.Response.WriteAsync($"Dynamic endpoint {path} added!");
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger.json", "Dynamic API v1");
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
