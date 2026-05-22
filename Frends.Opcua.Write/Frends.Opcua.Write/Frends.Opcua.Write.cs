using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.Opcua.Write.Definitions;
using Frends.Opcua.Write.Helpers;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using Opc.Ua.Client;

namespace Frends.Opcua.Write;

/// <summary>
/// Task Class for Opcua operations.
/// </summary>
public static class Opcua
{
    /// <summary>
    /// Task for writing data to OPCUA Server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Opcua-Write)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> Write(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            ValidationHandler.Run(input, connection, options);

            await using var session = await SessionFactory.CreateAsync(options, connection, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            WriteNode[] writeNodes;

            if (input.InputType is Enums.InputType.Json)
            {
                writeNodes = JArray.Parse(input.WriteNodesJson)
                    .Select(token => new WriteNode
                    {
                        NodeId = token["NodeId"]!.Value<string>(),
                        Value = token["Value"]?.ToObject<object>(),
                    })
                    .ToArray();
            }
            else
            {
                writeNodes = input.WriteNodes;
            }

            var resultArray = WriteNodes(session.Session, writeNodes);

            return new Result
            {
                Success = resultArray.Cast<JToken>().All(node => (bool)node["IsSuccess"]),
                Nodes = resultArray,
            };
        }
        catch (ServiceResultException ex)
        {
            return new Exception(CreateOpcUaErrorMessage(ex), ex).Handle(options);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options);
        }
    }

    private static JArray WriteNodes(Session session, WriteNode[] nodes)
    {
        // Build the list of nodes to read
        var nodesToWrite = new WriteValueCollection();
        foreach (var node in nodes)
        {
            nodesToWrite.Add(new WriteValue
            {
                NodeId = NodeId.Parse(node.NodeId),
                AttributeId = Opc.Ua.Attributes.Value,
                Value = new DataValue
                {
                    Value = node.Value,
                    StatusCode = StatusCodes.Good,
                    SourceTimestamp = DateTime.UtcNow,
                },
            });
        }

        // Perform the read
        session.Write(
            requestHeader: null,
            nodesToWrite: nodesToWrite,
            results: out StatusCodeCollection results,
            diagnosticInfos: out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToWrite);

        // Map results to JSON.
        var resultArray = new JArray();

        for (int i = 0; i < nodes.Length; i++)
        {
            var statusCode = results[i];

            var nodeResult = new JObject
            {
                ["NodeId"] = nodes[i].NodeId,
                ["StatusCode"] = statusCode.ToString(),
                ["IsSuccess"] = StatusCode.IsGood(statusCode),
            };

            if (!StatusCode.IsGood(statusCode))
            {
                nodeResult["Error"] = GetNodeStatusDescription(statusCode);
            }

            resultArray.Add(nodeResult);
        }

        return resultArray;
    }

    private static string CreateOpcUaErrorMessage(ServiceResultException ex)
    {
        return ex.StatusCode switch
        {
            StatusCodes.BadConnectionClosed => "Connection to the OPC UA server was closed unexpectedly.",
            StatusCodes.BadNotConnected => "Unable to connect to the OPC UA server. Verify the server URL and that the server is reachable.",
            StatusCodes.BadSecureChannelClosed => "Secure channel to the OPC UA server was closed.",
            StatusCodes.BadSecurityChecksFailed => "OPC UA security validation failed. Verify security mode, security policy, and certificates.",
            StatusCodes.BadCertificateUntrusted => "The OPC UA server certificate is not trusted.",
            StatusCodes.BadIdentityTokenInvalid => "The OPC UA server rejected the provided authentication token.",
            StatusCodes.BadIdentityTokenRejected => "The OPC UA server rejected the provided user identity.",
            StatusCodes.BadUserAccessDenied => "Access was denied for the provided credentials.",
            StatusCodes.BadSessionClosed => "The OPC UA session was closed unexpectedly.",
            StatusCodes.BadSessionIdInvalid => "The OPC UA session is invalid or expired.",
            StatusCodes.BadNodeIdUnknown => "One or more provided NodeIds do not exist on the OPC UA server.",
            StatusCodes.BadTimeout => "The OPC UA operation timed out.",
            _ => $"OPC UA error ({ex.StatusCode}): {ex.Message}",
        };
    }

    private static string GetNodeStatusDescription(StatusCode statusCode)
    {
        return statusCode.Code switch
        {
            StatusCodes.BadNodeIdUnknown => "The NodeId does not exist on the OPC UA server.",
            StatusCodes.BadNodeIdInvalid => "The NodeId format is invalid.",
            StatusCodes.BadNotReadable => "The node value is not readable.",
            StatusCodes.BadUserAccessDenied => "Access to the node was denied.",
            StatusCodes.BadWaitingForInitialData => "The server has not received initial data yet.",
            _ => $"OPC UA node write failed with status: {statusCode}",
        };
    }
}
