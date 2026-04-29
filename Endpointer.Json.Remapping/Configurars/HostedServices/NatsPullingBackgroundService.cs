using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Microsoft.Extensions.Options;

namespace Endpointer.Json.Remapping.Configurars.HostedServices;

public class NatsPullingBackgroundService : BackgroundService
{
    private readonly INatsConnection _nats;
    private readonly INatsJSContext _js;
    private readonly NatsOptions _opt;
    private readonly ILogger<NatsPullingBackgroundService> _logger;

    public NatsPullingBackgroundService(
        INatsConnection nats,
        INatsJSContext js,
        IOptions<NatsOptions> opt,
        ILogger<NatsPullingBackgroundService> logger)
    {
        _nats = nats;
        _js = js;
        _opt = opt.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var consumerName = $"{_opt.Name}_WORKER";
            _logger.LogInformation("Creating consumer with name: '{ConsumerName}'", consumerName);

            var consumer = await _js.CreateOrUpdateConsumerAsync(_opt.JsonMappingStreamName,
                new ConsumerConfig(consumerName)
                {
                    DeliverPolicy = ConsumerConfigDeliverPolicy.All,
                }, stoppingToken);

            _logger.LogInformation("Worker bound to stream {Stream}. Listening...", _opt.JsonMappingStreamName);

            await foreach (var msg in consumer.ConsumeAsync<string>(cancellationToken: stoppingToken))
            {
                try
                {
                    // Your remapping logic here...
                    // Example: var result = Remap(msg.Data);

                    // Manual Ack is required because of AckPolicy.Explicit
                    await msg.AckAsync(cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message {Seq}", msg.Metadata?.Sequence.Stream);
                    // Message will be redelivered by NATS after AckWait timeout
                }
            }
        }
        catch (OperationCanceledException) { /* Normal shutdown */ }
        catch (Exception ex)
        {
            throw ex;
            _logger.LogCritical(ex, "NATS Worker loop crashed.");
        }
    }
}