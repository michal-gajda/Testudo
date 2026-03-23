namespace Testudo.Worker;

using System;
using Testudo.Contracts.Interfaces;

/// <summary>
/// Handles callbacks from WCF duplex service
/// </summary>
public class ServiceCallbackHandler : IService1Callback
{
    private readonly ILogger<ServiceCallbackHandler> _logger;

    public ServiceCallbackHandler(ILogger<ServiceCallbackHandler> logger)
    {
        _logger = logger;
    }

    public void OnServerMessage(string message)
    {
        _logger.LogInformation("[WCF Callback] Server Message: {message}", message);
    }

    public void OnNotification(string notification)
    {
        _logger.LogInformation("[WCF Callback] Notification: {notification}", notification);
    }

    public void OnPong(string response)
    {
        _logger.LogInformation("[WCF Callback] Pong received: {response}", response);
    }
}
