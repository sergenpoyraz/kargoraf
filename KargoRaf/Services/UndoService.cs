using KargoRaf.Models;

namespace KargoRaf.Services;

public class UndoService
{
    private CancellationTokenSource? _cts;
    private Package? _lastDelivered;

    public event Action<Package>? UndoAvailable;
    public event Action? UndoExpired;

    public Package? LastDelivered => _lastDelivered;

    public void RegisterDelivery(Package package)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _lastDelivered = package;
        UndoAvailable?.Invoke(package);

        _ = ExpireAfterDelay(_cts.Token);
    }

    public Package? TryUndo(PackageService packageService)
    {
        if (_lastDelivered is null) return null;
        var pkg = packageService.RestorePackage(_lastDelivered);
        Clear();
        return pkg;
    }

    public void Clear()
    {
        _cts?.Cancel();
        _lastDelivered = null;
        UndoExpired?.Invoke();
    }

    private async Task ExpireAfterDelay(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            if (!token.IsCancellationRequested)
            {
                _lastDelivered = null;
                UndoExpired?.Invoke();
            }
        }
        catch (TaskCanceledException)
        {
            // Yeni teslim veya geri alma.
        }
    }
}
