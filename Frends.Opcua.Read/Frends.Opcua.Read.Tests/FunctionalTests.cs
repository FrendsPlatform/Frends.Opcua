using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frends.Opcua.Read.Definitions;
using Frends.Opcua.Read.Enums;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.Opcua.Read.Tests;

[TestFixture]
internal class FunctionalTests
{
    private Connection connectionAnonymous;
    private Connection connectionUsernamePassword;
    private Connection connectionSecure;
    private Options options;

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
    }

    [Test]
    public async Task Opcua_ReadWithAnonymousAccess()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Read,
            NodeIds = new string[] { "ns=3;s=StepUp", "ns=3;s=RandomSignedInt32" },
        };

        var result = await Opcua.Read(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
        Assert.That(((JToken)result.NodeValues).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.NodeValues).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");

        connectionAnonymous.Path = "opcplc-anonymous";

        result = await Opcua.Read(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
        Assert.That(((JToken)result.NodeValues).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.NodeValues).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
    }

    [Test]
    public async Task Opcua_ReadWithAnonymousAccessIPAddress()
    {
        connectionAnonymous.ServerName = "127.0.0.1";
        var input = new Input
        {
            Mode = OpcOperationMode.Read,
            NodeIds = new string[] { "ns=3;s=StepUp", "ns=3;s=RandomSignedInt32" },
        };

        var result = await Opcua.Read(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
        Assert.That(((JToken)result.NodeValues).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.NodeValues).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");

        connectionAnonymous.Path = "opcplc-anonymous";

        result = await Opcua.Read(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
        Assert.That(((JToken)result.NodeValues).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.NodeValues).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
    }

    [Test]
    public async Task Opcua_BrowseWithAnonymous()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Browse,
            StartNodeId = "i=85",
        };

        var result = await Opcua.Read(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);

        connectionAnonymous.Path = "opcplc-anonymous";

        result = await Opcua.Read(input, connectionAnonymous, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
    }

    [Test]
    public async Task Opcua_ReadWithUsernamePasswordAccess()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Read,
            NodeIds = new string[] { "ns=3;s=StepUp", "ns=3;s=RandomSignedInt32" },
        };

        var result = await Opcua.Read(input, connectionUsernamePassword, options, default);
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task Opcua_BrowseWithUsernamePasswordAccess()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Browse,
            StartNodeId = "i=85",
        };

        var result = await Opcua.Read(input, connectionUsernamePassword, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);

        Console.WriteLine(result.NodeValues);
    }

    [Test]
    public async Task Opcua_BrowseWithInvalidUsernamePassword()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Browse,
            StartNodeId = "i=85",
        };

        connectionUsernamePassword.Username = "invalid";
        connectionUsernamePassword.Password = "password";
        options.ThrowErrorOnFailure = false;

        var result = await Opcua.Read(input, connectionUsernamePassword, options, default);
        Assert.That(result.Error.Message, Does.Contain("Access was denied for the provided credentials."));
    }

    [Test]
    public async Task Opcua_ReadWithCertificateAccess()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Read,
            NodeIds = new string[] { "ns=3;s=StepUp", "ns=3;s=RandomSignedInt32" },
        };

        Console.WriteLine($"Volumes dir: {connectionSecure.CertificatePath}");
        Console.WriteLine($"Absolute path: {Path.GetFullPath(connectionSecure.CertificatePath)}");
        Console.WriteLine($"Volumes exists: {Directory.Exists(connectionSecure.CertificatePath)}");

        var result = await Opcua.Read(input, connectionSecure, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
        Assert.That(((JToken)result.NodeValues).All(node => (string)node["StatusCode"] == "Good"), $"Some nodes did not return Good status: {string.Join(", ", ((JToken)result.NodeValues).Where(node => (string)node["StatusCode"] != "Good").Select(node => $"{node["NodeId"]} = {node["StatusCode"]}"))}");
    }

    [Test]
    public async Task Opcua_BrowseWithCertificateAccess()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Browse,
            StartNodeId = "i=85",
        };

        var result = await Opcua.Read(input, connectionSecure, options, default);
        Assert.That(result.Success, Is.True);

        Assert.That(result.NodeValues, Is.Not.Empty);
    }

    [Test]
    public async Task Opcua_ReadWithNonExistingCertificate()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Browse,
            StartNodeId = "i=85",
        };

        connectionSecure.CertificatePath = Path.Combine(Path.GetTempPath(), "nonexistingcert.pfx");

        Func<Task> act = async () => await Opcua.Read(input, connectionSecure, options, default);

        var ex = Assert.ThrowsAsync<Exception>(act);
        Assert.That(ex!.Message, Does.Contain("Certificate inside parameter CertificatePath needs to exist."));
    }

    [Test]
    public async Task Opcua_ReadWithoutCertificate()
    {
        var input = new Input
        {
            Mode = OpcOperationMode.Browse,
            StartNodeId = "i=85",
        };

        connectionSecure.CertificatePath = string.Empty;

        Func<Task> act = async () => await Opcua.Read(input, connectionSecure, options, default);

        var ex = Assert.ThrowsAsync<Exception>(act);
        Assert.That(ex!.Message, Does.Contain("CertificatePath is required."));
    }
}
