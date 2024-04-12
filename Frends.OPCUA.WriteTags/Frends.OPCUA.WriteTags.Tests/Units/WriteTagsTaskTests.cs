using Frends.OPCUA.WriteTags.Definitions;
using Frends.OPCUA.WriteTags.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Frends.OPCUA.WriteTags.Tests.Units;

public class WriteTagsTaskTests
{

    [Fact]
    public async Task ReturnsSuccess_OnClientSuccess()
    {
        var client = Substitute
            .For<IOpcUaClient>();
        client.WriteDataAsync(Arg.Any<WriteTagValue[]>()).Returns(new WriteResult { IsAllSuccess = true });

        var factory = Substitute
            .For<IOpcUaClientFactory>();
        factory.CreateClient(Arg.Any<Input>(), Arg.Any<Options>())
            .Returns(client);

        OPCUA.ClientFactory = factory;

        var writeTags = await OPCUA.WriteTags(new Input(), new Options(), new CancellationToken());

        Assert.NotNull(writeTags);
        Assert.True(writeTags.IsAllSuccess);
    }

    [Fact]
    public async Task ReturnsFailure_OnClientError()
    {
        const string expectedMessage = "Error_from_client";
        var client = Substitute
            .For<IOpcUaClient>();
        client.WriteDataAsync(Arg.Any<WriteTagValue[]>()).Throws(new Exception(expectedMessage));

        var factory = Substitute
            .For<IOpcUaClientFactory>();
        factory.CreateClient(Arg.Any<Input>(), Arg.Any<Options>())
            .Returns(client);

        OPCUA.ClientFactory = factory;

        var writeTags = await OPCUA.WriteTags(new Input(), new Options(), new CancellationToken());

        Assert.False(writeTags.IsAllSuccess);
        Assert.Equal(expectedMessage, writeTags.Reason);
    }
}