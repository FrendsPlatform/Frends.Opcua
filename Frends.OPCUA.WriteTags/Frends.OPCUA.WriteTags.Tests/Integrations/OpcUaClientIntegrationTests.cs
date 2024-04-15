using Frends.OPCUA.WriteTags.Definitions;
using Frends.OPCUA.WriteTags.Services;

namespace Frends.OPCUA.WriteTags.Tests.Integrations;

public class OpcUaClientIntegrationTests
{
    private IOpcUaClientFactory _factory = new OpcUaClientFactory();

    private WriteTagValue _writeTagInt = new WriteTagValue
    {
        NodeId = "ns=2;s=Scalar_Static_Int32",
        Value = 100
    };

    [Fact]
    public async Task ThrowsError_OnBadUrl()
    {
        var invaidInputBadUrl = new Input
        {
            Url = "opc.tcp://localhost:62542/Quickstarts/ReferenceServer"
        };
        var options = new Options
        {
            Authentication = AuthenticationMethod.Anonymous,
            ApplicationName = "test suite",
            TrustServerCertificate = true
        };
        var sut = _factory.CreateClient(invaidInputBadUrl, options);

        await Assert.ThrowsAsync<OpcSessionException>(async () =>
        {
            await sut.WriteDataAsync(new[] { _writeTagInt });
        });
    }

    [Fact]
    public async Task ReturnsResult_OnAnonymousWriteRequest()
    {
        var input = new Input
        {
            Url = "opc.tcp://localhost:62541/Quickstarts/ReferenceServer"
        };
        var options = new Options
        {
            Authentication = AuthenticationMethod.Anonymous,
            ApplicationName = "test suite",
            TrustServerCertificate = true
        };
        var sut = _factory.CreateClient(input, options);

        var response = await sut.WriteDataAsync(new[] { _writeTagInt });

        Assert.NotNull(response);
        Assert.True(response.IsAllSuccess);
        Assert.NotEmpty(response.Results);
        Assert.Equal(_writeTagInt.NodeId, response.Results[0].NodeId);
        Assert.Equal(_writeTagInt.Value, response.Results[0].Value);
    }

    [Fact]
    public async Task ReturnsResult_OnUserAuthWriteRequest()
    {
        var input = new Input
        {
            Url = "opc.tcp://localhost:62541/Quickstarts/ReferenceServer"
        };
        var options = new Options
        {
            Authentication = AuthenticationMethod.UserIdentity,
            Username = "user1",
            Password = "password",
            ApplicationName = "test suite",
            TrustServerCertificate = true
        };
        var sut = _factory.CreateClient(input, options);

        var response = await sut.WriteDataAsync(new[] { _writeTagInt });

        Assert.NotNull(response);
        Assert.True(response.IsAllSuccess);
        Assert.NotEmpty(response.Results);
        Assert.Equal(_writeTagInt.NodeId, response.Results[0].NodeId);
        Assert.Equal(_writeTagInt.Value, response.Results[0].Value);
    }

    [Fact]
    public async Task Throws_OnInvalidRequest()
    {
        var input = new Input
        {
            Url = "opc.tcp://localhost:62541/Quickstarts/ReferenceServer"
        };
        var options = new Options
        {
            Authentication = AuthenticationMethod.Anonymous,
            ApplicationName = "test suite",
            TrustServerCertificate = true
        };
        var sut = _factory.CreateClient(input, options);

        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await sut.WriteDataAsync(new[] { new WriteTagValue { NodeId = "invalid_node_id", Value = "42" } });
        });
    }
}