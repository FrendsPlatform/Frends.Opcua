namespace Frends.Opcua.Read.Enums;

public enum OpcOperationMode
{
    /// <summary>
    /// Reads specific nodes specifyed in NodeIds parameter.
    /// </summary>
    Read,

    /// <summary>
    /// Browses existing nodes using StartNodeId parameter.
    /// </summary>
    Browse,
}
