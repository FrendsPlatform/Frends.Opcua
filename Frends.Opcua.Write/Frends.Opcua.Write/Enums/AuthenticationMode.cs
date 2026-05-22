namespace Frends.Opcua.Write.Enums;

public enum AuthenticationMode
{
    /// <summary>
    /// No authentication is used to connect.
    /// </summary>
    Anonymous,

    /// <summary>
    /// Username and password authentication is used.
    /// </summary>
    UsernamePassword,

    /// <summary>
    /// Certificate authentication is used.
    /// </summary>
    Certificate,
}
