using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.Opcua.Write.Attributes;
using Frends.Opcua.Write.Enums;

namespace Frends.Opcua.Write.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// OPC UA Server name or IP address.
    /// </summary>
    /// <example>localhost</example>
    [DefaultValue("localhost")]
    [Required]
    [DisplayFormat(DataFormatString = "Text")]
    public string ServerName { get; set; } = "localhost";

    /// <summary>
    /// Port to be used to connect to the OPC UA Server.
    /// </summary>
    /// <example>4080</example>
    [DefaultValue(4080)]
    [Required]
    public int Port { get; set; } = 4080;

    /// <summary>
    /// Optional parameter to set a specific path to the server URL.
    /// </summary>
    /// <example>path</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Whether to accept untrusted/self-signed server certificates.
    /// Set to false in production environments.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool AutoAcceptUntrustedCertificates { get; set; } = true;

    /// <summary>
    /// Authentication to be used connecting to the OPC UA Server.
    /// </summary>
    /// <example>AuthenticationMode.UsernamePassword</example>
    [DefaultValue(AuthenticationMode.UsernamePassword)]
    public AuthenticationMode Authentication { get; set; }

    /// <summary>
    /// Username for the authentication.
    /// </summary>
    /// <example>user</example>
    [DisplayFormat(DataFormatString = "Text")]
    [RequiredIf(nameof(Authentication), AuthenticationMode.UsernamePassword)]
    [UIHint(nameof(Authentication), "", AuthenticationMode.UsernamePassword)]
    public string Username { get; set; }

    /// <summary>
    /// Password for the authentication.
    /// </summary>
    /// <example>pass</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [RequiredIf(nameof(Authentication), AuthenticationMode.UsernamePassword)]
    [UIHint(nameof(Authentication), "", AuthenticationMode.UsernamePassword)]
    public string Password { get; set; }

    /// <summary>
    /// Path to the certificate to be used to authenticate to OPC UA Server.
    /// </summary>
    /// <example>C:\path\to\certificate</example>
    [DisplayFormat(DataFormatString = "Text")]
    [RequiredToExistIf(nameof(Authentication), true, AuthenticationMode.Certificate)]
    [UIHint(nameof(Authentication), "", AuthenticationMode.Certificate)]
    public string CertificatePath { get; set; }

    /// <summary>
    /// Password for the certificate file.
    /// </summary>
    /// <example>Password</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Authentication), "", AuthenticationMode.Certificate)]
    public string CertificatePassword { get; set; }

    /// <summary>
    /// Path to the private key. Used forr certificate authentication if .der or .crt typed certifications are being used.
    /// </summary>
    /// <example>C:\path\to\privatekey</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(Authentication), "", AuthenticationMode.Certificate)]
    public string PrivateKeyPath { get; set; }

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    /// <example>10</example>
    [DefaultValue(10)]
    public int ConnectionTimeout { get; set; } = 10;

    /// <summary>
    /// Session timeout in seconds.
    /// </summary>
    /// <example>60</example>
    [DefaultValue(60)]
    public int SessionTimeout { get; set; } = 60;

    /// <summary>
    /// Defines the OPC UA message security level applied to communication between client and server.
    /// Controls whether messages are sent without security, digitally signed for integrity,
    /// or both signed and encrypted for full confidentiality and protection against tampering.
    /// </summary>
    /// <example>OpcMessageSecurityMode.None</example>
    [DefaultValue(OpcMessageSecurityMode.None)]
    public OpcMessageSecurityMode SecurityMode { get; set; }

    /// <summary>
    /// Optional path to a PFX/PKCS#12 file to use as the client application certificate
    /// for establishing a secure channel. When left empty a temporary self-signed certificate
    /// is generated for the duration of the task and discarded afterwards.
    /// Requires ApplicationCertificatePassword if the PFX is password protected.
    /// </summary>
    /// <example>C:\path\to\cert</example>
    [UIHint(nameof(SecurityMode), "", OpcMessageSecurityMode.Sign, OpcMessageSecurityMode.SignAndEncrypt)]
    [DisplayFormat(DataFormatString = "Text")]
    public string ApplicationCertificatePath { get; set; }

    /// <summary>
    /// Password for the PFX file specified in ApplicationCertificatePath.
    /// Leave empty if the certificate has no password.
    /// </summary>
    /// <example>passphrase</example>
    [PasswordPropertyText]
    [UIHint(nameof(SecurityMode), "", OpcMessageSecurityMode.Sign, OpcMessageSecurityMode.SignAndEncrypt)]
    [DisplayFormat(DataFormatString = "Text")]
    public string ApplicationCertificatePassword { get; set; }

    /// <summary>
    /// Defines the cryptographic algorithm set used for securing OPC UA communication.
    /// Includes encryption algorithms, hashing functions, and key exchange mechanisms.
    /// The selected policy must be supported by the server endpoint and compatible with the chosen SecurityMode.
    /// </summary>
    /// <example>OpcSecurityPolicy.None</example>
    [DefaultValue(OpcSecurityPolicy.None)]
    public OpcSecurityPolicy SecurityPolicy { get; set; }
}
