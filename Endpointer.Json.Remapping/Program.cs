using Endpointer.Json.Remapping.Configurars;
using Endpointer.Json.Remapping.Configurars.S3;
using Endpointer.Json.Remapping.Services.JsonMapping;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();

services.AddSingleton<IJsonMapper, JsonMapper>();
services.AddSingleton<IJsonMappingRequestPreparar, JsonMappingRequestPreparar>();

new NatsConfigurar().ConfigureServices(builder);
new S3Configurar().ConfigureServices(builder);

var app = builder.Build();
await new NatsConfigurar().ConfigureAppAsync(app);
app.Run();
