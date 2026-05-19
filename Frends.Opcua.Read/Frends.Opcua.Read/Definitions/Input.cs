using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Opcua.Read.Attributes;
using Frends.Opcua.Read.Enums;

namespace Frends.Opcua.Read.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Determines the mode for either reading specific nodes or browse existing nodes.
    /// </summary>
    /// <example>OpcOperationMode.ReadNodes</example>
    [DefaultValue(OpcOperationMode.Read)]
    public OpcOperationMode Mode { get; set; }

    /// <summary>
    /// List of NodeIds to read from the OPC UA Server.
    /// </summary>
    /// <example>[ "ns=2;i=1001", "ns=2;s=Temperature" ]</example>
    [UIHint(nameof(Mode), "", OpcOperationMode.Read)]
    [RequiredIf(nameof(Mode), "", OpcOperationMode.Read)]
    public string[] NodeIds { get; set; }

    /// <summary>
    /// NodeId of the node where browsing should be started.
    /// </summary>
    /// <example>ns=2;s=Temperature</example>
    [UIHint(nameof(Mode), "", OpcOperationMode.Browse)]
    [RequiredIf(nameof(Mode), "", OpcOperationMode.Browse)]
    public string StartNodeId { get; set; }
}
