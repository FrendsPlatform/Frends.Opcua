namespace Frends.OPCUA.WriteTags.Definitions;

/// <summary>
/// Authentication method used to conenct to OPC UA server
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Anonymous connection type
    /// </summary>
    Anonymous,
    /// <summary>
    /// Client will authenticate using Username and Password 
    /// </summary>
    UserIdentity
}