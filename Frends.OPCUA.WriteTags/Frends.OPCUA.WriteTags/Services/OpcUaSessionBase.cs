using Opc.Ua;
using Opc.Ua.Client;

namespace Frends.OPCUA.WriteTags.Services;

internal abstract class OpcUaSessionBase
{
    protected OpcUaSessionBase(ApplicationConfiguration clientConfig, string serverUrl)
    {
        ClientConfig = clientConfig ?? throw new ArgumentException("Client config must be specified", nameof(clientConfig));
        ServerUrl = string.IsNullOrEmpty(serverUrl) ? throw new ArgumentException("Server URL must be specified", nameof(serverUrl)) : serverUrl;
    }

    private const uint SessionLifeTime = 60000;
    public ApplicationConfiguration ClientConfig { get; }
    public string ServerUrl { get; }
    public IUserIdentity? UserIdentity { get; set; }

    protected virtual async Task<ISession> CreateSessionAsync(CancellationToken ct = default)
    {
        try
        {
            var f = DefaultSessionFactory.Instance;

            ITransportWaitingConnection? connection = null;
            var endpointDescription = CoreClientUtils.SelectEndpoint(ClientConfig, ServerUrl, false);

            var endpointConfiguration = EndpointConfiguration.Create(ClientConfig);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            // creates session using default factory
            // can throw exception in case no valid connection can be made, ex. bad port, authentication, certificates
            // ConfigureAwait(false) adds slight performance boost and can avoid deadlocks when used with SynchronizationContext
            var session =
                await f.CreateAsync(
                    ClientConfig,
                    connection,
                    endpoint,
                    connection == null,
                    false,
                    ClientConfig.ApplicationName,
                    SessionLifeTime,
                    UserIdentity,
                    null,
                    ct
                ).ConfigureAwait(false);
            return session;
        }
        catch (Exception e)
        {
            throw new OpcSessionException("Could not create session", e);
        }
    }
}