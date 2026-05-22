using System.ComponentModel.DataAnnotations;

namespace Frends.Opcua.Write.Definitions;

/// <summary>
/// Nodes to be written into the Opcua server.
/// </summary>
public class WriteNode
{
    /// <summary>
    /// Id for the node to be written onto the Opcua server.
    /// </summary>
    /// <example>ns=3;s=StepUp</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string NodeId { get; set; }

    /// <summary>
    /// Value to be stored into the Node.
    /// </summary>
    /// <example>72.5</example>
    [DisplayFormat(DataFormatString = "Text")]
    public object Value { get; set; }
}
