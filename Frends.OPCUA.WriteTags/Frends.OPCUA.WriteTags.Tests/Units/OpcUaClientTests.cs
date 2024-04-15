using Frends.OPCUA.WriteTags.Definitions;
using Frends.OPCUA.WriteTags.Services;
using Opc.Ua;

namespace Frends.OPCUA.WriteTags.Tests.Units;

public class OpcUaClientTests
{
    private readonly string _localGoodUrl = "opc.tcp://localhost";
    [Fact]
    public void CreatesOpcUaClient()
    {
        var appConfig = new ApplicationConfiguration();

        var sut = new OpcUaClient(appConfig, _localGoodUrl);

        Assert.NotNull(sut);
        Assert.Equal(_localGoodUrl, sut.ServerUrl);
        Assert.Equal(appConfig, sut.ClientConfig);
    }

    [Fact]
    public void ThrowsArgumentEx_CtrOnNullAppConfig()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var sut = new OpcUaClient(null, _localGoodUrl);
        });
    }

    [Fact]
    public void ThrowsArgumentEx_CtrOnNullUrl()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var sut = new OpcUaClient(new ApplicationConfiguration(), null);
        });
    }

    [Fact]
    public void WriteDataThrowsArgumentEx_OnNullData()
    {
        var appConfig = new ApplicationConfiguration();

        var sut = new OpcUaClient(appConfig, _localGoodUrl);

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await sut.WriteDataAsync(null);
        });
    }

    [Fact]
    public void WriteDataThrowsArgumentEx_OnEmptyData()
    {
        var appConfig = new ApplicationConfiguration();

        var sut = new OpcUaClient(appConfig, _localGoodUrl);

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await sut.WriteDataAsync(Array.Empty<WriteTagValue>());
        });
    }
}