namespace Frends.Opcua.Write.Enums;

/// <summary>
/// Defines the cryptographic security algorithm suite used for securing OPC UA communication.
/// This includes encryption algorithms, key exchange mechanisms, and signature algorithms.
/// </summary>
public enum OpcSecurityPolicy
{
    /// <summary>
    /// No security policy is applied.
    /// Communication is unencrypted and unsigned.
    /// This should only be used in development or fully trusted environments.
    /// </summary>
    None,

    /// <summary>
    /// Legacy security policy using RSA encryption with SHA-256 hashing.
    /// Widely supported by industrial OPC UA servers and older PLC implementations.
    /// </summary>
    Basic256Sha256,

    /// <summary>
    /// Modern security policy using AES-128 encryption, SHA-256 hashing, and RSA-OAEP key exchange.
    /// Provides strong security and is widely used in current OPC UA implementations.
    /// </summary>
    Aes128Sha256RsaOaep,

    /// <summary>
    /// High-security policy using AES-256 encryption, SHA-256 hashing, and RSA-PSS signatures.
    /// Designed for environments requiring maximum cryptographic strength and compliance.
    /// </summary>
    Aes256Sha256RsaPss,
}
