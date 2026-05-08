using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Opcua.Read.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    [Obsolete]
    public async Task Should_Throw_Error_When_ThrowErrorOnFailure_Is_True(Exception exception)
    {
        AsyncTestDelegate call = async () =>
    await Opcua.Read(DefaultInput(), DefaultConnection(), DefaultOptions(), default);

        var ex = Assert.ThrowsAsync<Exception>(call);
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = await Opcua.Read(DefaultInput(), DefaultConnection(), options, default);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    [Obsolete]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        var ex = Assert.ThrowsAsync<Exception>((AsyncTestDelegate)(() =>
            Opcua.Read(DefaultInput(), DefaultConnection(), options, default)));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}
