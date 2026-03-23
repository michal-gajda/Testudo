namespace Testudo.Contracts.Interfaces
{
    using System.ServiceModel;

    /// <summary>
    /// Service contract with bidirectional (duplex) communication support
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IService1Callback))]
    public interface IService1Duplex
    {
        /// <summary>
        /// Subscribe to server notifications (duplex operation)
        /// </summary>
        [OperationContract]
        void Subscribe();

        /// <summary>
        /// Unsubscribe from server notifications (duplex operation)
        /// </summary>
        [OperationContract]
        void Unsubscribe();
    }
}
