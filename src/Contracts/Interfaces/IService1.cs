namespace Testudo.Contracts.Interfaces
{
    using System.ServiceModel;
    using Testudo.Contracts.Models;

    /// <summary>
    /// Service contract for basic operations
    /// </summary>
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        string Ping();

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        /// <summary>
        /// Broadcasts a message to all subscribed duplex clients (Workers)
        /// </summary>
        [OperationContract]
        int BroadcastToSubscribers(string message);
    }
}
