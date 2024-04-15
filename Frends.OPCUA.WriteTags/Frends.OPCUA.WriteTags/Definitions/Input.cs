using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OPCUA.WriteTags.Definitions;

/// <summary>
/// Task input parameters
/// </summary>
public class Input
{
    /// <summary>
    /// The URL with protocol and path.
    /// </summary>
    /// <example>opc.tcp://localhost:62541/Quickstarts/ReferenceServer</example>
    [DefaultValue("opc.tcp://localhost:62541/Quickstarts/ReferenceServer")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Url { get; set; }

    /// <summary>
    /// Array of values to be written to server.
    /// </summary>
    /// <example>[
    /// {NodeId = "ns=2;s=some_value", Value = "42"},
    /// {NodeId = "ns=2;s=some_other_value", Value = 42}
    /// ]</example>
    public WriteTagValue[] WriteValues { get; set; }
}

/// <summary>
/// Description of values which will be send to OPC UA server
/// </summary>
public class WriteTagValue
{
    /// <summary>
    /// NodeId in string format, for details please refer to OPC Specifications Address Space Model sections 7.2 and 5.2.2
    /// </summary>
    public string NodeId { get; set; }
    /// <summary>
    /// Value to be written
    /// </summary>
    public object Value { get; set; }
}