using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.Opcua.Write.Definitions;
using Frends.Opcua.Write.Enums;
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
        // Read DataType attributes for all nodes first
        var nodesToReadTypes = new ReadValueIdCollection(
            nodes.Select(n => new ReadValueId
            {
                NodeId = NodeId.Parse(n.NodeId),
                AttributeId = Opc.Ua.Attributes.DataType,
            }));

        session.Read(null, 0, TimestampsToReturn.Neither, nodesToReadTypes, out DataValueCollection typeResults, out DiagnosticInfoCollection _);

        // Build the list of nodes to write
        var nodesToWrite = new WriteValueCollection();

        for (var i = 0; i < nodes.Length; i++)
        {
            var dataTypeId = typeResults[i].Value as NodeId;
            var coercedValue = CoerceValue(nodes[i].Value, dataTypeId);

            nodesToWrite.Add(new WriteValue
            {
                NodeId = NodeId.Parse(nodes[i].NodeId),
                AttributeId = Opc.Ua.Attributes.Value,
                Value = new DataValue
                {
                    Value = coercedValue,
                    StatusCode = StatusCodes.Good,
                    SourceTimestamp = DateTime.UtcNow,
                },
            });
        }

        // Perform the write
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
            StatusCodes.BadNotWritable => "The node is not writable.",
            _ => $"OPC UA node write failed with status: {statusCode}",
        };
    }

    private static object CoerceValue(object value, NodeId dataTypeId)
    {
        if (value == null || dataTypeId == null)
            return value;

        // Convert to long first as a common intermediate from JSON deserialization
        try
        {
            var builtInType = TypeInfo.GetBuiltInType(dataTypeId);
            return builtInType switch
            {
                BuiltInType.Boolean => Convert.ToBoolean(value),
                BuiltInType.SByte => Convert.ToSByte(value),
                BuiltInType.Byte => Convert.ToByte(value),
                BuiltInType.Int16 => Convert.ToInt16(value),
                BuiltInType.UInt16 => Convert.ToUInt16(value),
                BuiltInType.Int32 => Convert.ToInt32(value),
                BuiltInType.UInt32 => Convert.ToUInt32(value),
                BuiltInType.Int64 => Convert.ToInt64(value),
                BuiltInType.UInt64 => Convert.ToUInt64(value),
                BuiltInType.Float => Convert.ToSingle(value),
                BuiltInType.Double => Convert.ToDouble(value),
                BuiltInType.String => Convert.ToString(value),
                BuiltInType.DateTime => Convert.ToDateTime(value),
                _ => value, // Pass through for complex types
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to coerce value '{value}' ({value.GetType().Name}) to OPC UA type '{dataTypeId}': {ex.Message}", ex);
        }
    }
}
