using System.ComponentModel;
using Frends.OPCUA.WriteTags.Definitions;
using Frends.OPCUA.WriteTags.Services;

namespace Frends.OPCUA.WriteTags;

/// <summary>
/// Main static class for WriteTags Task
/// </summary>
public static class OPCUA
{
    /// <summary>
    /// Static client factory used to instantiate OPC UA client, used also in tests
    /// </summary>
    public static IOpcUaClientFactory ClientFactory { get; set; } = new OpcUaClientFactory();

    /// <summary>
    /// Main task method responsible for writing tags to OPC UA server
    /// </summary>
    /// <param name="input">Input parameters from Task configuration</param>
    /// <param name="options">Options parameters from Task configuration</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<WriteResult> WriteTags([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        var result = new WriteResult();
        var client = ClientFactory.CreateClient(input, options);

        try
        {
            result = await client.WriteDataAsync(input.WriteValues, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            result.IsAllSuccess = false;
            result.Reason = e.Message;
        }

        return result;
    }
}