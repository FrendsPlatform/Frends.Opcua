using Frends.OPCUA.WriteTags.Definitions;

namespace Frends.OPCUA.WriteTags.Services;

/// <summary>
/// Client service to access OPC UA server
/// </summary>
public interface IOpcUaClient
{
    /// <summary>
    /// Writes tags to OPC UA server
    /// </summary>
    /// <param name="writeValues">Array of tags to be sent to server</param>
    /// <param name="ct">Token used to cancel operation</param>
    /// <returns>Summary information about status, data written and eventually reason of failure</returns>
    Task<WriteResult> WriteDataAsync(WriteTagValue[] writeValues, CancellationToken ct = default);
}