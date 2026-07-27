namespace Frends.Opcua.Write.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the read operation was successful.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// JSON array of node write results.
    /// Each element contains NodeId, Value, DataType, StatusCode, and SourceTimestamp.
    /// </summary>
    /// <example>{ }</example>
    public dynamic Nodes { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
