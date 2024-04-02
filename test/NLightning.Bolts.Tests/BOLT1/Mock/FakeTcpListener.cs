using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NLightning.Bolts.Tests.BOLT1.Mock;

public class FakeTcpListener(IPAddress address, int port) : TcpListener(address, port)
{
    public new async Task<TcpClient> AcceptTcpClientAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate delay or waiting for connection attempt that will be canceled.
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Debug.Print("FakeTcpListener Task Canceled.");
        }

        return null; // Or return a mock/fake TcpClient as needed.
    }
}