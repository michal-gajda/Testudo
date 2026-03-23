namespace Testudo.Cli;

using System;
using System.ServiceModel;
using Testudo.Contracts.Interfaces;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║  WCF Service Client - Console App      ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        string serviceUrl = "http://localhost:57870/Service1.svc";
        Console.WriteLine($"Connecting to: {serviceUrl}\n");

        try
        {
            // Create WCF client using ChannelFactory with BasicHttpBinding (simple communication)
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress(serviceUrl);
            var factory = new ChannelFactory<IService1>(binding, endpoint);
            var client = factory.CreateChannel();

            // Test GetData operation
            Console.WriteLine("Testing GetData operation:");
            Console.WriteLine("─────────────────────────────────────");
            int testValue = 42;
            string result = client.GetData(testValue);
            Console.WriteLine("✓ Successfully connected to WCF service!\n");
            Console.WriteLine($"Input: {testValue}");
            Console.WriteLine($"Output: {result}\n");

            // Test GetDataUsingDataContract operation
            Console.WriteLine("Testing GetDataUsingDataContract operation:");
            Console.WriteLine("─────────────────────────────────────");
            var compositeData = new Testudo.Contracts.Models.CompositeType
            {
                BoolValue = true,
                StringValue = "Test"
            };
            Console.WriteLine($"Input: BoolValue={compositeData.BoolValue}, StringValue={compositeData.StringValue}");

            var resultData = client.GetDataUsingDataContract(compositeData);
            Console.WriteLine($"Output: BoolValue={resultData.BoolValue}, StringValue={resultData.StringValue}\n");

            // Close connection
            ((IClientChannel)client).Close();
            Console.WriteLine("✓ Connection closed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}

