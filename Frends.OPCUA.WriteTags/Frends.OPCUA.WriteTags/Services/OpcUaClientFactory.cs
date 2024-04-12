using Frends.OPCUA.WriteTags.Definitions;
using Opc.Ua;

namespace Frends.OPCUA.WriteTags.Services;

internal class OpcUaClientFactory : IOpcUaClientFactory
{
    public IOpcUaClient CreateClient(Input inputData, Options clientOptions)
    {
        var appConfig = new ApplicationConfiguration
        {
            ApplicationType = ApplicationType.Client,
            ApplicationName = clientOptions.ApplicationName,
            SecurityConfiguration = new SecurityConfiguration(),
            CertificateValidator = new CertificateValidator(),
            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = 60000,
                WellKnownDiscoveryUrls = new StringCollection
                {
                    "opc.tcp://{0}:4840",
                    "http://{0}:52601/UADiscovery",
                    "http://{0}/UADiscovery/Default.svc"
                },
                DiscoveryServers = null,
                MinSubscriptionLifetime = 10000,
                OperationLimits = new OperationLimits
                {
                    MaxNodesPerRead = 2500,
                    MaxNodesPerHistoryReadData = 1000,
                    MaxNodesPerHistoryReadEvents = 1000,
                    MaxNodesPerWrite = 2500,
                    MaxNodesPerHistoryUpdateData = 1000,
                    MaxNodesPerHistoryUpdateEvents = 1000,
                    MaxNodesPerMethodCall = 2500,
                    MaxNodesPerBrowse = 2500,
                    MaxNodesPerRegisterNodes = 2500,
                    MaxNodesPerTranslateBrowsePathsToNodeIds = 2500,
                    MaxNodesPerNodeManagement = 2500,
                    MaxMonitoredItemsPerCall = 2500,
                }
            }
        };

        appConfig.CertificateValidator.CertificateValidation += (sender, eventArgs) =>
        {
            eventArgs.Accept = ShouldAcceptCertificate(clientOptions, eventArgs.Certificate.Thumbprint);
        };

        var client = new OpcUaClient(appConfig, inputData.Url);

        if (clientOptions.Authentication == AuthenticationMethod.UserIdentity)
        {
            client.UserIdentity = new UserIdentity(clientOptions.Username, clientOptions.Password);
        }

        return client;
    }

    public bool ShouldAcceptCertificate(Options options, string certThumbprint)
    {
        if (options.TrustServerCertificate)
        {
            return true;
        }

        return certThumbprint.Equals(options.TrustedCertificateThumbprint,
            StringComparison.InvariantCultureIgnoreCase);
    }
}