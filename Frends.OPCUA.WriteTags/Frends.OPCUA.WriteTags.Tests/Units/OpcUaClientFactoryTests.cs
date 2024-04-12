using Frends.OPCUA.WriteTags.Definitions;
using Frends.OPCUA.WriteTags.Services;

namespace Frends.OPCUA.WriteTags.Tests.Units;

public class OpcUaClientFactoryTests
{
    [Fact]
    public void CreatesOpcUaClient()
    {
        var input = new TestTaskInput();
        var options = new Options();

        var sut = new OpcUaClientFactory();

        var client = sut.CreateClient(input, options);

        Assert.NotNull(client);
        Assert.IsType<OpcUaClient>(client);
    }

    [Fact]
    public void SetsClientIdentity_OnUserIdentityOption()
    {
        var input = new TestTaskInput();
        var options = new Options
        {
            Authentication = AuthenticationMethod.UserIdentity,
            Username = "test",
            Password = "test"
        };

        var sut = new OpcUaClientFactory();

        var client = sut.CreateClient(input, options);

        Assert.NotNull(client);
        var opcUaClient = Assert.IsType<OpcUaClient>(client);

        Assert.NotNull(opcUaClient.UserIdentity);
    }

    [Fact]
    public void AcceptCert_OnTrustServerCertOption()
    {
        var input = new TestTaskInput();
        var options = new Options
        {
            TrustServerCertificate = true
        };

        var sut = new OpcUaClientFactory();

        Assert.True(sut.ShouldAcceptCertificate(options, ""));
    }

    [Fact]
    public void RejectCert_OnInvalidServerCertOption()
    {
        var input = new TestTaskInput();
        var options = new Options
        {
            TrustServerCertificate = false,
            TrustedCertificateThumbprint = "123456"
        };

        var sut = new OpcUaClientFactory();

        Assert.False(sut.ShouldAcceptCertificate(options, ""));
    }
}