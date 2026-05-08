using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.Opcua.Read.Definitions;
using Frends.Opcua.Read.Enums;
using Frends.Opcua.Read.Factories;
using Frends.Opcua.Read.Helpers;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using Opc.Ua.Client;

namespace Frends.Opcua.Read;

/// <summary>
/// Task Class for Opcua operations.
/// </summary>
public static class Opcua
{
    /// <summary>
    /// Task for reading data from OPCUA Server
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Opcua-Read)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> Read(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        ValidateInput(input, connection);

        try
        {
            await using var session = await SessionFactory.CreateAsync(options, connection, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var collectedNodes = new List<string>();
            var visited = new HashSet<string>();

            if (input.Mode == OpcOperationMode.Browse)
            {
                BrowseRecursive(
                session.Session,
                ObjectIds.ObjectsFolder,
                collectedNodes,
                visited);
            }
            else
            {
                collectedNodes = input.NodeIds.ToList();
            }

            var resultArray = ReadNodes(session.Session, collectedNodes);

            return new Result
            {
                Success = true,
                NodeValues = resultArray,
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

    private static void ValidateInput(Input input, Connection connection)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (string.IsNullOrWhiteSpace(connection.ServerName))
            throw new ArgumentException("ServerName must not be empty.", nameof(connection.ServerName));
        if (input.Mode == OpcOperationMode.Read && (input.NodeIds == null || input.NodeIds.Count() == 0))
            throw new ArgumentException("NodeIds list must contain at least one NodeId.", nameof(input.NodeIds));
        if (input.Mode == OpcOperationMode.Browse && string.IsNullOrWhiteSpace(input.StartNodeId))
            throw new ArgumentException("StartNodeId must be provided.", nameof(input.StartNodeId));
        if (connection.Authentication == AuthenticationMode.Certificate)
        {
            if (string.IsNullOrWhiteSpace(connection.CertificatePath))
                throw new ArgumentException("CertificatePath is required for Certificate authentication.", nameof(connection.CertificatePath));
            if (!File.Exists(connection.CertificatePath))
                throw new FileNotFoundException("Certificate file not found.", connection.CertificatePath);
            if (connection.SecurityMode != OpcMessageSecurityMode.SignAndEncrypt)
                throw new ArgumentException("Certificate authentication requires SecurityMode SignAndEncrypt.");
        }

        if (connection.SecurityMode != OpcMessageSecurityMode.None &&
            !string.IsNullOrWhiteSpace(connection.ApplicationCertificatePath) &&
            !File.Exists(connection.ApplicationCertificatePath))
            throw new FileNotFoundException("Application certificate file not found.", connection.ApplicationCertificatePath);
    }

    private static JArray ReadNodes(Session session, List<string> nodeIds)
    {
        // Build the list of nodes to read
        var nodesToRead = new ReadValueIdCollection();
        foreach (var nodeIdStr in nodeIds)
        {
            nodesToRead.Add(new ReadValueId
            {
                NodeId = NodeId.Parse(nodeIdStr),
                AttributeId = Opc.Ua.Attributes.Value,
            });
        }

        // Perform the read
        session.Read(
            requestHeader: null,
            maxAge: 0,
            timestampsToReturn: TimestampsToReturn.Source,
            nodesToRead: nodesToRead,
            results: out DataValueCollection dataValues,
            diagnosticInfos: out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(dataValues, nodesToRead);

        // Map results to JSON
        var resultArray = new JArray();

        for (int i = 0; i < nodeIds.Count; i++)
        {
            var dataValue = dataValues[i];
            var statusCode = dataValue.StatusCode;

            var nodeResult = new JObject
            {
                ["NodeId"] = nodeIds[i],
                ["StatusCode"] = statusCode.ToString(),
                ["IsSuccess"] = StatusCode.IsGood(statusCode),
            };

            if (StatusCode.IsGood(statusCode))
            {
                nodeResult["Value"] = dataValue.Value != null
                    ? JToken.FromObject(dataValue.Value)
                    : JValue.CreateNull();

                nodeResult["DataType"] =
                    dataValue.Value?.GetType().Name ?? "null";

                nodeResult["SourceTimestamp"] =
                    dataValue.SourceTimestamp == DateTime.MinValue
                        ? JValue.CreateNull()
                        : dataValue.SourceTimestamp.ToString("o");
            }
            else
            {
                nodeResult["Error"] =
                    GetNodeStatusDescription(statusCode);
            }

            resultArray.Add(nodeResult);
        }

        return resultArray;
    }

    private static void BrowseRecursive(
    Session session,
    NodeId nodeId,
    List<string> collectedNodes,
    ISet<string> visited)
    {
        // Prevent loops (OPC UA can have cyclic references)
        var key = nodeId.ToString();
        if (!visited.Add(key))
            return;

        var browseDescription = new BrowseDescription
        {
            NodeId = nodeId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
            ResultMask = (uint)BrowseResultMask.All,
        };

        session.Browse(
            null,
            null,
            0,
            new BrowseDescriptionCollection { browseDescription },
            out BrowseResultCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        if (results == null || results.Count == 0 || results[0].References == null)
            return;

        foreach (var reference in results[0].References)
        {
            // Convert ExpandedNodeId → NodeId
            var childNodeId = ExpandedNodeId.ToNodeId(
                reference.NodeId,
                session.NamespaceUris);

            if (childNodeId == null)
                continue;

            // Only collect Variable nodes (values you can read)
            if (reference.NodeClass == NodeClass.Variable)
                collectedNodes.Add(childNodeId.ToString());

            // Recurse into Objects / folders
            if (reference.NodeClass == NodeClass.Object || reference.NodeClass == NodeClass.Method)
                BrowseRecursive(session, childNodeId, collectedNodes, visited);
        }
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
            _ => $"OPC UA node read failed with status: {statusCode}",
        };
    }
}
