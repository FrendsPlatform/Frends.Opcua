using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.Opcua.Read.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        Func<Task> act = async () => await Opcua.Read(DefaultInput(), DefaultConnection(), DefaultOptions(), default);

        var ex = Assert.ThrowsAsync<Exception>(act);
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
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        Func<Task> act = async () => await Opcua.Read(DefaultInput(), DefaultConnection(), options, default);

        var ex = Assert.ThrowsAsync<Exception>(act);
        Assert.That(ex!.Message, Does.Contain(CustomErrorMessage));
    }
}
