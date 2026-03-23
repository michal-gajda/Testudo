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
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
    }
}
