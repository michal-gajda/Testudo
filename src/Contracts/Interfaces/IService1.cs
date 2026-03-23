namespace Testudo.Contracts.Interfaces
{
    using System.ServiceModel;
    using Testudo.Contracts.Models;

    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
    }
}
