using System.Runtime.CompilerServices;
using Frends.OPCUA.WriteTags.Definitions;
using Opc.Ua;

[assembly: InternalsVisibleTo("Frends.OPCUA.WriteTags.Tests")]
namespace Frends.OPCUA.WriteTags.Services;

internal class OpcUaClient : OpcUaSessionBase, IOpcUaClient
{
    public OpcUaClient(ApplicationConfiguration clientConfig, string serverUrl) : base(clientConfig, serverUrl)
    {
    }

    public async Task<WriteResult> WriteDataAsync(WriteTagValue[] writeValues, CancellationToken ct = default)
    {
        ThrowIfWriteValuesNotValid(writeValues);

        var result = new WriteResult();
        var session = await CreateSessionAsync(ct).ConfigureAwait(false);

        ct.ThrowIfCancellationRequested();

        try
        {
            var toBeWritten = writeValues.Select(value => new WriteValue
            {
                NodeId = new NodeId(value.NodeId),
                AttributeId = Attributes.Value,
                Value = new DataValue
                {
                    Value = value.Value
                }
            });

            // ConfigureAwait(false) adds slight performance boost and can avoid deadlocks when used with SynchronizationContext
            var response = await session
                .WriteAsync(null, new WriteValueCollection(toBeWritten), ct)
                .ConfigureAwait(false);

            //map to writeResult
            result.Results = writeValues.Select((valueResult, i) => new WriteValueResult
            {
                NodeId = valueResult.NodeId,
                Value = valueResult.Value,
                ResultCode = response.Results[i].ToString()
            }).ToArray();
            result.IsAllSuccess = result.Results.All(r => r.ResultCode.Equals("Good", StringComparison.InvariantCultureIgnoreCase));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            session?.CloseAsync(true, ct).ConfigureAwait(false);
            session?.Dispose();
        }

        return result;
    }

    private void ThrowIfWriteValuesNotValid(WriteTagValue[] writeValues)
    {
        if (writeValues == null)
        {
            throw new ArgumentException("Values to be written is null", nameof(writeValues));
        }

        if (!writeValues.Any())
        {
            throw new ArgumentException("Values to be written are empty", nameof(writeValues));
        }
    }
}