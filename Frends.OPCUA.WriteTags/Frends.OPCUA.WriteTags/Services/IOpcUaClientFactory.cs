using Frends.OPCUA.WriteTags.Definitions;

namespace Frends.OPCUA.WriteTags.Services;

/// <summary>
/// Interface used to create IOpcUaClient 
/// </summary>
public interface IOpcUaClientFactory
{
    /// <summary>
    /// Creates new IOpcUaClient instance based on input and options 
    /// </summary>
    /// <param name="inputData">Input parameters for creation of Client, proper URL is required</param>
    /// <param name="clientOptions">Client options</param>
    /// <returns>Instance of client ready to make a request</returns>
    IOpcUaClient CreateClient(Input inputData, Options clientOptions);
}