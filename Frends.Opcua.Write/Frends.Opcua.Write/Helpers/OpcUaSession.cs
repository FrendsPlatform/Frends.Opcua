using System;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace Frends.Opcua.Write.Helpers;

/// <summary>
/// A disposable wrapper around an OPC UA <see cref="Session"/> that closes
/// and disposes the session when it goes out of scope.
/// </summary>
internal sealed class OpcUaSession : IAsyncDisposable
{
    internal OpcUaSession(Session session)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
    }

    internal Session Session { get; }

    /// <summary>
    /// Disposes the session object.
    /// </summary>
    /// <returns>A completed once the session has been closed and disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Session.Connected)
            await Session.CloseAsync().ConfigureAwait(false);

        Session.Dispose();
    }
}
