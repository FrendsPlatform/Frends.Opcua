namespace Frends.Opcua.Write.Enums;

/// <summary>
/// Defines how OPC UA messages are secured at the transport level.
/// This controls whether messages are signed, encrypted, or sent in plain text.
/// </summary>
public enum OpcMessageSecurityMode
{
    /// <summary>
    /// No message security is applied. Messages are sent in plain text without signing or encryption.
    /// This mode provides no confidentiality or integrity protection and should only be used in trusted environments such as development or isolated networks.
    /// </summary>
    None,

    /// <summary>
    /// Messages are digitally signed to ensure data integrity and authenticity.
    /// The content is not encrypted, meaning it can still be read in transit.
    /// This mode protects against tampering but does not provide confidentiality.
    /// </summary>
    Sign,

    /// <summary>
    /// Messages are both signed and encrypted.
    /// This provides full confidentiality, integrity, and authentication of OPC UA communication.
    /// This is the recommended mode for production and untrusted networks.
    /// </summary>
    SignAndEncrypt,
}
