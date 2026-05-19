using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Opcua.Read.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Application name used when connecting to the OPC UA Server.
    /// </summary>
    /// <example>Frends.OpcUa.Client</example>
    [DefaultValue("Frends.OpcUa.Client")]
    [DisplayFormat(DataFormatString = "Text")]
    public string ApplicationName { get; set; }

    /// <summary>
    /// The root directory path where the OPC UA client PKI store is located.
    /// The SDK will create the following subdirectories automatically:
    /// <list type="bullet">
    /// <item><description><c>own</c> — stores the auto-generated client application certificate</description></item>
    /// <item><description><c>trusted</c> — stores trusted server certificates</description></item>
    /// <item><description><c>rejected</c> — stores rejected server certificates</description></item>
    /// </list>
    /// On first connect the SDK generates a self-signed application certificate and saves it
    /// under <c>{PkiRootPath}/own</c>, which is then reused on subsequent runs.
    /// Only applicable when OpcMessageSecurityMode is anything other than <c>None</c>.
    /// </summary>
    /// <example>./pki</example>
    public string PkiRootPath { get; set; }

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
