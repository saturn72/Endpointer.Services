using Endpointer.Json.Remapping.Configurars.HostedServices;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;

namespace Endpointer.Json.Remapping.Configurars;

public class NatsConfigurar
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        // 1. Setup Options
        services.AddOptions<NatsOptions>()
            .Bind(builder.Configuration.GetSection(NatsOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 2. Manual Registration (Since AddNats doesn't support DI resolution)
        services.AddSingleton<INatsConnection>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<NatsOptions>>().Value;
            var natsOpts = NatsOpts.Default with
            {
                Url = opt.Url,
                Name = opt.Name,
                RetryOnInitialConnect = true,
                ConnectTimeout = TimeSpan.FromSeconds(5)
            };
            return new NatsConnection(natsOpts);
        });

        // 3. Register JS Context and Worker
        services.AddSingleton(sp => sp.GetRequiredService<INatsConnection>().CreateJetStreamContext());
        services.AddHostedService<NatsPullingBackgroundService>();
    }

    public async Task ConfigureAppAsync(WebApplication app)
    {
        var nats = app.Services.GetRequiredService<INatsConnection>();
        var js = app.Services.GetRequiredService<INatsJSContext>();
        var logger = app.Services.GetRequiredService<ILogger<NatsConfigurar>>();
        var opt = app.Services.GetRequiredService<IOptions<NatsOptions>>().Value;

        var shutdownToken = app.Lifetime.ApplicationStopping;
        var maxRetries = byte.MaxValue;

        for (int i = 1; i <= maxRetries; i++)
        {
            if (shutdownToken.IsCancellationRequested) return;

            try
            {
                logger.LogInformation("NATS Warmup Attempt {Attempt}/{Max}", i, maxRetries);

                // Probe the TCP connection directly
                var connectionState = nats.ConnectionState;
                if (connectionState != NatsConnectionState.Open)
                {
                    await nats.ConnectAsync();
                }

                _ = await js.GetAccountInfoAsync(shutdownToken);

                var sn = opt.JsonMappingStreamName;
                await js.CreateOrUpdateStreamAsync(new StreamConfig(
                    name: sn,
                    subjects: [$"{sn}.>"]
                ), shutdownToken);

                logger.LogInformation("NATS Warmup Successful.");
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (i == maxRetries) throw;

                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, i), 30));
                logger.LogWarning(ex, "NATS not ready (attempt {Attempt}/{Max}). Retrying in {Delay}s...",
                    i, maxRetries, delay.TotalSeconds);

                await Task.Delay(delay, shutdownToken);
            }
        }
    }
}