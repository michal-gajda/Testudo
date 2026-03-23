namespace Testudo.Contracts.Interfaces
{
    using System.ServiceModel;

    /// <summary>
    /// Callback interface for bidirectional (duplex) communication with clients
    /// </summary>
    [ServiceContract]
    public interface IService1Callback
    {
        /// <summary>
        /// Called by the service to send messages back to the client
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnServerMessage(string message);

        /// <summary>
        /// Called by the service to notify the client of events
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnNotification(string notification);

        /// <summary>
        /// Called by the service in response to a duplex Ping()
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnPong(string response);
    }
}
