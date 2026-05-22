using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Frends.Opcua.Write.Definitions;
using Frends.Opcua.Write.Enums;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.Opcua.Write.Tests;

[TestFixture]
internal class FunctionalTests
{
    private Connection connectionAnonymous;
    private Connection connectionUsernamePassword;
    private Connection connectionSecure;
    private Options options;
    private Input input;

    [SetUp]
    public void Setup()
    {
        connectionAnonymous = new Connection
        {
            ServerName = "localhost",
            Port = 50000,
            Path = string.Empty,
            AutoAcceptUntrustedCertificates = true,
            Authentication = AuthenticationMode.Anonymous,
            ConnectionTimeout = 10,
            SessionTimeout = 60,
            SecurityMode = OpcMessageSecurityMode.None,
            SecurityPolicy = OpcSecurityPolicy.None,
        };

        connectionUsernamePassword = new Connection
        {
            ServerName = "localhost",
            Port = 50001,
            Path = string.Empty,
            AutoAcceptUntrustedCertificates = true,
            Authentication = AuthenticationMode.UsernamePassword,
            ConnectionTimeout = 10,
            SessionTimeout = 60,
            SecurityMode = OpcMessageSecurityMode.Sign,
            SecurityPolicy = OpcSecurityPolicy.Aes256Sha256RsaPss,
            Username = "admin",
            Password = "admin",
            ApplicationCertificatePath = null,
            ApplicationCertificatePassword = null,
        };

        connectionSecure = new Connection
        {
            ServerName = "localhost",
            Port = 50002,
            Path = string.Empty,
            AutoAcceptUntrustedCertificates = true,
            Authentication = AuthenticationMode.Certificate,
            ConnectionTimeout = 10,
            SessionTimeout = 60,
            SecurityMode = OpcMessageSecurityMode.SignAndEncrypt,
            SecurityPolicy = OpcSecurityPolicy.Basic256Sha256,
            CertificatePassword = "yourpassword",
            CertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/user.pfx"),
            PrivateKeyPath = string.Empty, // Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/trusted-user/certs/user.key"),
        };

        options = new Options
        {
            ApplicationName = "Frends.OpcUa.Client",
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = string.Empty,
        };

        input = new Input
        {
            InputType = InputType.WriteNodes,
            WriteNodes = new WriteNode[]
            {
                  new WriteNode() { NodeId = "ns=3;s=WriteTest_Int32",  Value = RandomNumberGenerator.GetInt32(1000), },
                  new WriteNode() { NodeId = "ns=3;s=WriteTest_Double", Value = new Random().NextDouble(), },
                  new WriteNode() { NodeId = "ns=3;s=WriteTest_Bool",   Value = new Random().Next() > (int.MaxValue / 2), },
                  new WriteNode() { NodeId = "ns=3;s=WriteTest_String", Value = Guid.NewGuid().ToString("n")[8..], },
            },
        };
    }

    [Test]
    public async Task Opcua_WriteWithAnonymousAccess()
    {
        var result = await Opcua.Write(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.Nodes, Is.Not.Empty);
        Assert.That(((JToken)result.Nodes).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.Nodes).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
        connectionAnonymous.Path = "opcplc-anonymous";

        result = await Opcua.Write(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.Nodes, Is.Not.Empty);
        Assert.That(((JToken)result.Nodes).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.Nodes).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
    }

    [Test]
    public async Task Opcua_WriteWithAnonymousAccessIPAddress()
    {
        connectionAnonymous.ServerName = "127.0.0.1";

        var result = await Opcua.Write(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.Nodes, Is.Not.Empty);
        Assert.That(((JToken)result.Nodes).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.Nodes).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");

        connectionAnonymous.Path = "opcplc-anonymous";

        result = await Opcua.Write(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.Nodes, Is.Not.Empty);
        Assert.That(((JToken)result.Nodes).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.Nodes).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
    }

    [Test]
    public async Task Opcua_WriteWithUsernamePasswordAccess()
    {
        var result = await Opcua.Write(input, connectionUsernamePassword, options, default);
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task Opcua_WriteWithInvalidUsernamePassword()
    {
        connectionUsernamePassword.Username = "invalid";
        connectionUsernamePassword.Password = "password";
        options.ThrowErrorOnFailure = false;

        var result = await Opcua.Write(input, connectionUsernamePassword, options, default);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Access was denied for the provided credentials."));
    }

    [Test]
    public async Task Opcua_WriteWithCertificateAccess()
    {
        Console.WriteLine($"Volumes dir: {connectionSecure.CertificatePath}");
        Console.WriteLine($"Absolute path: {Path.GetFullPath(connectionSecure.CertificatePath)}");
        Console.WriteLine($"Volumes exists: {Directory.Exists(connectionSecure.CertificatePath)}");

        var result = await Opcua.Write(input, connectionSecure, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.Nodes, Is.Not.Empty);
        Assert.That(((JToken)result.Nodes).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.Nodes).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
    }

    [Test]
    public async Task Opcua_WriteWithNonExistingCertificate()
    {
        connectionSecure.CertificatePath = Path.Combine(Path.GetTempPath(), "nonexistingcert.pfx");

        Func<Task> act = async () => await Opcua.Write(input, connectionSecure, options, default);

        var ex = Assert.ThrowsAsync<Exception>(act);
        Assert.That(ex!.Message, Does.Contain("Certificate inside parameter CertificatePath needs to exist."));
    }

    [Test]
    public async Task Opcua_WriteWithoutCertificate()
    {
        connectionSecure.CertificatePath = string.Empty;

        Func<Task> act = async () => await Opcua.Write(input, connectionSecure, options, default);

        var ex = Assert.ThrowsAsync<Exception>(act);
        Assert.That(ex!.Message, Does.Contain("CertificatePath is required."));
    }
}
