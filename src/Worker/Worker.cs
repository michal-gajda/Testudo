namespace Testudo.Worker;

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Testudo.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    private IService1Duplex? _client;
    private readonly string _serviceUrl = configuration["WcfService:DuplexUrl"]
        ?? throw new InvalidOperationException("WcfService:DuplexUrl is not configured");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker service starting at: {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectToService(stoppingToken);

                // Main loop - keep running and listen for server messages
                while (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
                    await Task.Delay(30000, stoppingToken);
                }
            }
            catch (OperationCanceledException ex)
            {
                logger.LogDebug(ex, "Worker shutdown requested, stopping");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Worker connection error: {Message}. Retrying in 10 seconds...", ex.Message);
                await DisconnectFromService();

                try { await Task.Delay(10000, stoppingToken); }
                catch (OperationCanceledException cancelEx)
                {
                    logger.LogDebug(cancelEx, "Worker shutdown requested during retry delay, stopping");
                    break;
                }
            }
        }

        await DisconnectFromService();
        logger.LogInformation("Worker service stopped at: {Time}", DateTimeOffset.Now);
    }

    private async Task ConnectToService(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Attempting to connect to WCF service at: {Url}", _serviceUrl);

            // Create callback handler for duplex communication
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var callbackLogger = loggerFactory.CreateLogger<ServiceCallbackHandler>();
            var callbackHandler = new ServiceCallbackHandler(callbackLogger);
            var context = new InstanceContext(callbackHandler);

            // Create WCF binding for duplex HTTP (WebSocket) communication
            var binding = new NetHttpBinding
            {
                ReceiveTimeout = TimeSpan.FromMinutes(30),
                SendTimeout = TimeSpan.FromMinutes(30),
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 65536
            };

            // Create endpoint address
            var endpoint = new EndpointAddress(_serviceUrl);

            // Create duplex channel factory with callback context
            var factory = new DuplexChannelFactory<IService1Duplex>(context, binding, endpoint);
            _client = factory.CreateChannel();

            // Subscribe to receive server notifications
            _client.Subscribe();

            logger.LogInformation("Successfully connected to WCF service and subscribed to notifications");

            // Ping to verify duplex callback channel is working — response arrives via OnPong()
            _client.Ping();
            logger.LogInformation("Ping sent — waiting for Pong callback...");

            // Wait a bit for initial server responses (subscription confirmation + pong)
            await Task.Delay(1000, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to WCF service: {Message}", ex.Message);
            throw;
        }
    }

    private async Task DisconnectFromService()
    {
        try
        {
            if (_client is IClientChannel channel)
            {
                logger.LogInformation("Disconnecting from WCF service");

                if (channel.State == CommunicationState.Opened)
                {
                    try
                    {
                        _client.Unsubscribe();
                        channel.Close();
                        logger.LogInformation("Successfully disconnected from WCF service");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to close WCF channel gracefully, aborting");
                        channel.Abort();
                    }
                }
                else
                {
                    logger.LogWarning("WCF channel is in {State} state, aborting", channel.State);
                    channel.Abort();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while disconnecting from WCF service: {Message}", ex.Message);
        }
        finally
        {
            _client = null;
        }
    }
}
