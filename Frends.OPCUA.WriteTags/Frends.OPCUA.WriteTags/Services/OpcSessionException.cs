namespace Frends.OPCUA.WriteTags.Services;

/// <summary>
/// Exception thrown when client cannot create session to OPC UA servr
/// </summary>
public class OpcSessionException : Exception
{
    /// <summary>
    /// Error constructor for session-failed exception
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public OpcSessionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}