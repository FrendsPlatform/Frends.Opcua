using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Opcua.Write.Attributes;
using Frends.Opcua.Write.Enums;

namespace Frends.Opcua.Write.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Determines how the WriteNodes will be handed to the Task.
    /// </summary>
    /// <example>InputType.WriteNodes</example>
    public InputType InputType { get; set; }

    /// <summary>
    /// Array of WriteNode objects with NodeId and Value for the Node to be written to the Opcua server.
    /// </summary>
    /// <example>[{"NodeId": "ns=2;s=MyDevice.Temperature", "Value": 72.5}]</example>
    [UIHint(nameof(InputType), "", InputType.WriteNodes)]
    [RequiredIf(nameof(InputType), InputType.WriteNodes)]
    public WriteNode[] WriteNodes { get; set; }

    /// <summary>
    /// JSON array of nodes to write. Notice that the schema of JSON needs to be exact as the example.
    /// </summary>
    /// <example>[{"NodeId": "ns=2;s=MyDevice.Temperature", "Value": 72.5}]</example>
    [UIHint(nameof(InputType), "", InputType.Json)]
    [RequiredIf(nameof(InputType), InputType.Json)]
    [ValidJsonArray("NodeId", "Value")]
    [DisplayFormat(DataFormatString = "Text")]
    public string WriteNodesJson { get; set; }
}
