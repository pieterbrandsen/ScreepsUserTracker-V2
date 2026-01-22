namespace UserTrackerConsole;

internal sealed class ConsoleLifetime : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public ConsoleLifetime()
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    public CancellationToken Token => _cts.Token;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Console.CancelKeyPress -= OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        _cts.Cancel();
        _cts.Dispose();
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        RequestStop();
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        RequestStop();
    }

    private void RequestStop()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }
}
