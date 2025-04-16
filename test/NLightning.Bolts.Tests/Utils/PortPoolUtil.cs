namespace NLightning.Bolts.Tests.Utils;

public static class PortPoolUtil
{
    private static readonly Random s_random = new();
    private static readonly HashSet<int> s_availablePorts = [];
    private static readonly SemaphoreSlim s_semaphore = new(50, 50);
    private static readonly object s_lock = new();

    static PortPoolUtil()
    {
        // Initialize the pool with a range of ports.
        for (var port = 49100; port < 49150; port++)
        {
            s_availablePorts.Add(port);
        }
    }

    public static async Task<int> GetAvailablePortAsync()
    {
        // Tests should no take more than 10 seconds to get a port.
        if (!await s_semaphore.WaitAsync(TimeSpan.FromSeconds(10)))
        {
            throw new TimeoutException("Could not get a port in time.");
        }

        lock (s_lock)
        {
            if (s_availablePorts.Count == 0)
            {
                throw new InvalidOperationException("No available ports. Are you returning them?");
            }

            var port = s_availablePorts.ToList()[s_random.Next(s_availablePorts.Count)];
            s_availablePorts.Remove(port);

            return port;
        }
    }

    public static void ReleasePort(int port)
    {
        lock (s_lock)
        {
            s_availablePorts.Add(port);
        }

        s_semaphore.Release();
    }
}