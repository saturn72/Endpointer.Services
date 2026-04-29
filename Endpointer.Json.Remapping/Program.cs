using Endpointer.Json.Remapping.Configurars;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

new NatsConfigurar().ConfigureServices(builder);

var app = builder.Build();
await new NatsConfigurar().ConfigureAppAsync(app);
app.Run();
