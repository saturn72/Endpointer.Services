using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Endpointer.Json.Remapping.Domain;
using Endpointer.Json.Remapping.Services.JsonMapping;

namespace Endpointer.Json.Remapping.Configurars.HostedServices;

public class JsonMappingPullingBackgroundService : BackgroundService
{
    private readonly INatsJSContext _js;
    private readonly NatsOptions _opt;
    private readonly IJsonMapper _jsonMapper;
    private readonly IJsonMappingRequestPreparar _preparar;
    private readonly ILogger<JsonMappingPullingBackgroundService> _logger;

    public JsonMappingPullingBackgroundService(
        INatsJSContext js,
        NatsOptions opt,
        IJsonMapper jsonMapper,
        IJsonMappingRequestPreparar preparar,
        ILogger<JsonMappingPullingBackgroundService> logger)
    {
        _js = js;
        _opt = opt;
        _jsonMapper = jsonMapper;
        _preparar = preparar;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var consumerName = $"{_opt.Name}_WORKER";
            _logger.LogInformation("Creating consumer with name: '{ConsumerName}'", consumerName);

            var consumer = await _js.CreateOrUpdateConsumerAsync(_opt.JsonMappingStreamName,
                new ConsumeConfig(consumerName)
                {
                    DeliverPolicy = ConsumerConfigDeliverPolicy.All,
                }, stoppingToken);

            _logger.LogInformation("Worker bound to stream {Stream}. Listening...", _opt.JsonMappingStreamName);

            var tasks = new List<Task>();
            await foreach (var msg in consumer.FetchNoWaitAsync<JsonMappingRequest>(
                            new NatsJSFetchOpts { MaxMsgs = _opt.MaxConcurrentMessages },
                            cancellationToken: stoppingToken))
                tasks.Add(ProcessAsync(msg));
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) { /* Normal shutdown */ }
    }

    private async Task ProcessAsync(INatsJSMsg<JsonMappingRequest> msg)
    {
        var data = msg.Data;
        if (data is null)
        {
            _logger.LogWarning("Received message with no data. Acking and skipping.");
            await msg.AckAsync();
            return;
        }

        var (isValid, messages) = await _preparar.PrepareAsync(data);
        if (!isValid)
        {
            _logger.LogWarning("Invalid JsonMappingRequest received: {Messages}", string.Join(", ", messages ?? []));
            throw new InvalidOperationException("Invalid JsonMappingRequest received --> move to debug queue");
        }

        await _jsonMapper.MapAsync(data);
        await msg.AckAsync();
    }
}