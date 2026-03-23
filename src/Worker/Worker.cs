namespace Testudo.Worker;

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Testudo.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private IService1Duplex? _client;
    private ServiceCallbackHandler? _callbackHandler;
    private string _serviceUrl = "http://localhost:57870/Service1.svc/duplex";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker service starting at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectToService(stoppingToken);

                // Main loop - keep running and listen for server messages
                while (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(30000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Worker connection error: {message}. Retrying in 10 seconds...", ex.Message);
                await DisconnectFromService();

                try { await Task.Delay(10000, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        await DisconnectFromService();
        logger.LogInformation("Worker service stopped at: {time}", DateTimeOffset.Now);
    }

    private async Task ConnectToService(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Attempting to connect to WCF service at: {url}", _serviceUrl);

            // Create callback handler for duplex communication  
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var callbackLogger = loggerFactory.CreateLogger<ServiceCallbackHandler>();
            _callbackHandler = new ServiceCallbackHandler(callbackLogger);
            var context = new InstanceContext(_callbackHandler);

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

            // Wait a bit for initial server response
            await Task.Delay(1000, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to WCF service: {message}", ex.Message);
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
                    catch
                    {
                        channel.Abort();
                    }
                }
                else
                {
                    channel.Abort();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while disconnecting from WCF service: {message}", ex.Message);
        }
        finally
        {
            _client = null;
        }
    }
}
