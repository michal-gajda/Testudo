namespace Testudo.WcfService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using Testudo.Contracts.Interfaces;

    /// <summary>
    /// Manages client callback channels for duplex communication
    /// </summary>
    public class ClientCallbackManager
    {
        private static readonly Lazy<ClientCallbackManager> _instance =
            new Lazy<ClientCallbackManager>(() => new ClientCallbackManager());

        private readonly Dictionary<string, IService1Callback> _clients =
            new Dictionary<string, IService1Callback>();

        private readonly object _lockObject = new object();

        public static ClientCallbackManager Instance => _instance.Value;

        /// <summary>
        /// Register a client callback channel
        /// </summary>
        public string RegisterClient(IService1Callback callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            string clientId = Guid.NewGuid().ToString();

            lock (_lockObject)
            {
                _clients[clientId] = callback;
            }

            return clientId;
        }

        /// <summary>
        /// Unregister a client callback channel
        /// </summary>
        public void UnregisterClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                return;

            lock (_lockObject)
            {
                _clients.Remove(clientId);
            }
        }

        /// <summary>
        /// Send a message to all connected clients
        /// </summary>
        public void BroadcastMessage(string message)
        {
            List<KeyValuePair<string, IService1Callback>> clientsCopy;

            lock (_lockObject)
            {
                clientsCopy = _clients.ToList();
            }

            foreach (var client in clientsCopy)
            {
                try
                {
                    client.Value.OnServerMessage(message);
                }
                catch (CommunicationException)
                {
                    // Client connection is broken, unregister it
                    UnregisterClient(client.Key);
                }
                catch (Exception ex)
                {
                    // Log exception and continue with other clients
                    System.Diagnostics.Debug.WriteLine($"Error sending message to client {client.Key}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Send a notification to all connected clients
        /// </summary>
        public void BroadcastNotification(string notification)
        {
            List<KeyValuePair<string, IService1Callback>> clientsCopy;

            lock (_lockObject)
            {
                clientsCopy = _clients.ToList();
            }

            foreach (var client in clientsCopy)
            {
                try
                {
                    client.Value.OnNotification(notification);
                }
                catch (CommunicationException)
                {
                    // Client connection is broken, unregister it
                    UnregisterClient(client.Key);
                }
                catch (Exception ex)
                {
                    // Log exception and continue with other clients
                    System.Diagnostics.Debug.WriteLine($"Error sending notification to client {client.Key}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the number of connected clients
        /// </summary>
        public int GetConnectedClientsCount()
        {
            lock (_lockObject)
            {
                return _clients.Count;
            }
        }

        /// <summary>
        /// Clear all client connections
        /// </summary>
        public void ClearAllClients()
        {
            lock (_lockObject)
            {
                _clients.Clear();
            }
        }
    }
}
