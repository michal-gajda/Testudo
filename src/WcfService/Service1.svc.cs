namespace Testudo.WcfService
{
    using System;
    using System.ServiceModel;
    using Testudo.Contracts.Interfaces;
    using Testudo.Contracts.Models;

    /// <summary>
    /// Service implementation with bidirectional (duplex) communication support
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Service1 : IService1, IService1Duplex
    {
        private string _clientId;
        private IService1Callback _callback;

        public Service1()
        {
            _clientId = null;
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }

            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }

            return composite;
        }

        /// <summary>
        /// Broadcasts a message to all currently subscribed duplex clients
        /// </summary>
        public int BroadcastToSubscribers(string message)
        {
            int count = ClientCallbackManager.Instance.GetConnectedClientsCount();
            ClientCallbackManager.Instance.BroadcastMessage(message);
            return count;
        }

        /// <summary>
        /// Subscribe the client to receive server notifications
        /// </summary>
        public void Subscribe()
        {
            try
            {
                _callback = OperationContext.Current.GetCallbackChannel<IService1Callback>();

                if (_callback == null)
                {
                    throw new InvalidOperationException("Unable to establish callback channel");
                }

                // Register this client in the callback manager
                _clientId = ClientCallbackManager.Instance.RegisterClient(_callback);

                // Notify the client about successful subscription
                _callback.OnNotification($"Client {_clientId} successfully subscribed to notifications");

                // Notify all other clients about the new subscription
                ClientCallbackManager.Instance.BroadcastNotification($"New client connected. Total connected clients: {ClientCallbackManager.Instance.GetConnectedClientsCount()}");

                System.Diagnostics.Debug.WriteLine($"Client {_clientId} subscribed. Total clients: {ClientCallbackManager.Instance.GetConnectedClientsCount()}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Subscribe error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unsubscribe the client from server notifications
        /// </summary>
        public void Unsubscribe()
        {
            try
            {
                if (!string.IsNullOrEmpty(_clientId))
                {
                    ClientCallbackManager.Instance.UnregisterClient(_clientId);

                    // Notify remaining clients about the disconnection
                    ClientCallbackManager.Instance.BroadcastNotification($"Client disconnected. Total connected clients: {ClientCallbackManager.Instance.GetConnectedClientsCount()}");

                    System.Diagnostics.Debug.WriteLine($"Client {_clientId} unsubscribed. Total clients: {ClientCallbackManager.Instance.GetConnectedClientsCount()}");

                    _clientId = null;
                    _callback = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unsubscribe error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Destructor to ensure cleanup on service instance disposal
        /// </summary>
        ~Service1()
        {
            if (!string.IsNullOrEmpty(_clientId))
            {
                ClientCallbackManager.Instance.UnregisterClient(_clientId);
            }
        }
    }
}
