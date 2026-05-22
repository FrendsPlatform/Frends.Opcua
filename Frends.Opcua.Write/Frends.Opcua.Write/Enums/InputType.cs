namespace Frends.Opcua.Write.Enums;

public enum InputType
{
    /// <summary>
    /// Input type to allow the WriteNodes to be given as WriteNodes objects.
    /// </summary>
    WriteNodes,

    /// <summary>
    /// Input type to allow the WriteNodes to be given as JSON string.
    /// </summary>
    Json,
}
