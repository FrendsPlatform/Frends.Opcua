using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.OPCUA.WriteTags.Definitions;

/// <summary>
/// Task options parameter
/// </summary>
public class Options
{
    /// <summary>
    /// Name of application to be assigned to OPC UA client
    /// </summary>
    /// <example>FrendsTask-001</example>
    [DefaultValue("FrendsTask")]
    public string ApplicationName { get; set; }

    /// <summary>
    /// Set if all server certificates should be trusted
    /// </summary>
    [DefaultValue(true)]
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Set thumbprint of trusted certificate, used when TrustServerCertificates is set to false
    /// </summary>
    /// <example>4095d7a9ea98e84ad5372f30a894afdae5962206</example>
    [DefaultValue("")]
    public string TrustedCertificateThumbprint { get; set; }

    /// <summary>
    /// Method of client authentication
    /// </summary>
    [DefaultValue(AuthenticationMethod.Anonymous)]
    public AuthenticationMethod Authentication { get; set; }

    /// <summary>
    /// If UserIdentity is selected you should fill Username and Password fields
    /// </summary>
    ///<example>user</example>
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.UserIdentity)]
    public string Username { get; set; }

    /// <summary>
    /// If UserIdentity is selected you should fill Username and Password fields
    /// </summary>
    /// <example>password1</example>
    [PasswordPropertyText]
    [UIHint(nameof(AuthenticationMethod), "", AuthenticationMethod.UserIdentity)]
    public string Password { get; set; }
}