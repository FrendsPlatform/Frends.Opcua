using Frends.Opcua.Read.Definitions;
using Frends.Opcua.Read.Enums;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Security.Certificates;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Opcua.Read.Factories;

internal static class SessionFactory
{
    internal static async Task<OpcUaSession> CreateAsync(
    Options options,
    Connection connection,
    CancellationToken cancellationToken)
    {
        var config = await BuildApplicationConfiguration(options, connection, cancellationToken);
        var serverUrl = $"opc.tcp://{connection.ServerName}:{connection.Port}";

        var endpointConfig = EndpointConfiguration.Create(config);
        endpointConfig.MaxMessageSize = 4194304;
        endpointConfig.MaxByteStringLength = 1048576;
        endpointConfig.MaxArrayLength = 65536;

        using var discoveryClient = DiscoveryClient.Create(new Uri(serverUrl), endpointConfig);
        var endpoints = discoveryClient.GetEndpoints(null);

        var selectedEndpoint = endpoints.FirstOrDefault(ep =>
            ep.SecurityMode == GetMessageSecurityMode(connection.SecurityMode) &&
            ep.SecurityPolicyUri == GetSecurityPolicy(connection.SecurityPolicy))
            ?? throw new InvalidOperationException(
                $"No endpoint found matching SecurityMode={connection.SecurityMode} " +
                $"and SecurityPolicy={connection.SecurityPolicy}.");

        var configuredEndpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfig);

        await configuredEndpoint.UpdateFromServerAsync(
            new Uri(serverUrl),
            GetMessageSecurityMode(connection.SecurityMode),
            GetSecurityPolicy(connection.SecurityPolicy),
            cancellationToken).ConfigureAwait(false);

        var userIdentity = await CreateUserIdentity(connection);

        var session = await Session.Create(
            config,
            configuredEndpoint,
            updateBeforeConnect: false,
            sessionName: options.ApplicationName,
            sessionTimeout: (uint)(connection.SessionTimeout * 1000),
            identity: userIdentity,
            preferredLocales: null).ConfigureAwait(false);

        return new OpcUaSession(session);
    }

    private static async Task<ApplicationConfiguration> BuildApplicationConfiguration(
    Options options,
    Connection connection,
    CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "opcua-client");

        var config = new ApplicationConfiguration
        {
            ApplicationName = options.ApplicationName,
            ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{options.ApplicationName}",
            ApplicationType = ApplicationType.Client,

            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier(),
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(tempPath, "trusted"),
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(tempPath, "issuer"),
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(tempPath, "rejected"),
                },
                AutoAcceptUntrustedCertificates = connection.AutoAcceptUntrustedCertificates,
                AddAppCertToTrustedStore = false,
            },

            TransportConfigurations = new TransportConfigurationCollection(),

            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = connection.ConnectionTimeout * 1000,
                MaxStringLength = 1048576,
                MaxByteStringLength = 1048576,
                MaxArrayLength = 65536,
                MaxMessageSize = 4194304,
                MaxBufferSize = 65536,
            },

            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = connection.SessionTimeout * 1000,
            },

            TraceConfiguration = new TraceConfiguration(),
        };

        // Always accept all certificates — no trust store on disk needed
        config.CertificateValidator = new CertificateValidator();
        config.CertificateValidator.CertificateValidation += (sender, e) => e.Accept = true;

        await config.Validate(ApplicationType.Client);

        if (connection.SecurityMode != OpcMessageSecurityMode.None)
        {
            config.SecurityConfiguration.ApplicationCertificate.Certificate =
                LoadOrCreateApplicationCertificate(config, connection, options);
        }

        return config;
    }

    private static async Task<IUserIdentity> CreateUserIdentity(Connection connection)
    {
        return connection.Authentication switch
        {
            AuthenticationMode.Anonymous => new UserIdentity(new AnonymousIdentityToken()),
            AuthenticationMode.UsernamePassword => new UserIdentity(connection.Username, connection.Password),
            AuthenticationMode.Certificate => new UserIdentity(LoadCertificate(connection)),
            _ => throw new ArgumentOutOfRangeException(nameof(connection.Authentication), connection.Authentication, null),
        };
    }

    private static X509Certificate2 LoadCertificate(Connection connection)
    {
        var ext = Path.GetExtension(connection.CertificatePath).ToLowerInvariant();

        return ext switch
        {
            ".pfx" or ".p12" => new X509Certificate2(
                connection.CertificatePath,
                connection.CertificatePassword,
                X509KeyStorageFlags.Exportable),

            ".der" or ".crt" => LoadDerWithPrivateKey(connection),

            _ => throw new NotSupportedException($"Certificate format '{ext}' is not supported. Use .pfx, .p12, .der, or .crt."),
        };
    }

    private static X509Certificate2 LoadDerWithPrivateKey(Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.PrivateKeyPath))
            throw new ArgumentException("PrivateKeyPath is required when using a DER/CRT certificate.");

        var cert = new X509Certificate2(connection.CertificatePath);
        var privateKey = RSA.Create();
        privateKey.ImportFromPem(File.ReadAllText(connection.PrivateKeyPath));
        return cert.CopyWithPrivateKey(privateKey);
    }

    private static X509Certificate2 LoadOrCreateApplicationCertificate(
    ApplicationConfiguration config,
    Connection connection,
    Options options)
    {
        if (!string.IsNullOrWhiteSpace(connection.ApplicationCertificatePath))
        {
            if (!File.Exists(connection.ApplicationCertificatePath))
                throw new FileNotFoundException("Application certificate file not found.", connection.ApplicationCertificatePath);

            return new X509Certificate2(
                connection.ApplicationCertificatePath,
                connection.ApplicationCertificatePassword,
                X509KeyStorageFlags.Exportable);
        }

        // No cert supplied — generate a temporary one for this run
        return CertificateBuilder
            .Create($"CN={options.ApplicationName.Replace(" ", "-")}")
            .SetNotBefore(DateTime.UtcNow - TimeSpan.FromDays(1))
            .SetLifeTime(CertificateFactory.DefaultLifeTime)
            .SetRSAKeySize(CertificateFactory.DefaultKeySize)
            .CreateForRSA();
    }

    private static string GetSecurityPolicy(OpcSecurityPolicy policy)
    {
        return policy switch
        {
            OpcSecurityPolicy.None => SecurityPolicies.None,
            OpcSecurityPolicy.Basic256Sha256 => SecurityPolicies.Basic256Sha256,
            OpcSecurityPolicy.Aes128Sha256RsaOaep => SecurityPolicies.Aes128_Sha256_RsaOaep,
            OpcSecurityPolicy.Aes256Sha256RsaPss => SecurityPolicies.Aes256_Sha256_RsaPss,
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null),
        };
    }

    private static MessageSecurityMode GetMessageSecurityMode(OpcMessageSecurityMode mode)
    {
        return mode switch
        {
            OpcMessageSecurityMode.None => MessageSecurityMode.None,
            OpcMessageSecurityMode.Sign => MessageSecurityMode.Sign,
            OpcMessageSecurityMode.SignAndEncrypt => MessageSecurityMode.SignAndEncrypt,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };
    }
}