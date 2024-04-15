namespace Frends.OPCUA.WriteTags.Definitions;

/// <summary>
/// Summary of write operation
/// </summary>
public class WriteResult
{
    /// <summary>
    /// Indicates that all writes were OK
    /// </summary>
    public bool IsAllSuccess { get; set; }

    /// <summary>
    /// Array of WriteValueResult for each write operation
    /// </summary>
    public WriteValueResult[] Results { get; set; } = Array.Empty<WriteValueResult>();

    /// <summary>
    /// Optional error description in case of failed writes
    /// </summary>
    public string Reason { get; set; } = String.Empty;
}

/// <summary>
/// Summary of specified operation
/// </summary>
public class WriteValueResult
{
    /// <summary>
    /// NodeId where was write attempt
    /// </summary>
    public string NodeId { get; set; }

    /// <summary>
    /// Value to be written
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Status of operation, can be Good, Uncertain or Bad
    /// </summary>
    public string ResultCode { get; set; }
}