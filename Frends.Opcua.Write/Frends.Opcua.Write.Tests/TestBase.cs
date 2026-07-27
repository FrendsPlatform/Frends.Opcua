using Frends.Opcua.Write.Definitions;

namespace Frends.Opcua.Write.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
