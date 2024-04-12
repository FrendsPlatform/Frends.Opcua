using Frends.OPCUA.WriteTags.Definitions;

namespace Frends.OPCUA.WriteTags.Tests.Units;

public class TestTaskInput : Input
{
    public TestTaskInput()
    {
        Url = "opc.tcp://localhost:62541/Quickstarts/ReferenceServer";
    }
}