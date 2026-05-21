using Frends.Opcua.Read.Definitions;

namespace Frends.Opcua.Read.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
