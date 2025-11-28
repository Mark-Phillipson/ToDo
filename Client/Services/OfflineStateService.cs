using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp.Client.Services
{
    public class OfflineStateService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private Timer? _timer;
        private bool _initialized;
        public bool IsOffline { get; private set; }
        public event Action<bool>? StatusChanged;
        private const int PollIntervalMs = 5000;

        public OfflineStateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            await CheckAndNotifyAsync();
            _timer = new Timer(async _ => await SafePoll(), null, PollIntervalMs, PollIntervalMs);
            _initialized = true;
        }

        private async Task SafePoll()
        {
            try { await CheckAndNotifyAsync(); } catch { /* ignore */ }
        }

        private async Task CheckAndNotifyAsync()
        {
            bool online;
            try
            {
                // Use existing JS helper if available, else fallback to navigator.onLine
                online = await _jsRuntime.InvokeAsync<bool>("blazorOnlineStatus.isOnline");
            }
            catch
            {
                online = true; // assume online if JS unavailable
            }
            var newOffline = !online;
            if (newOffline != IsOffline)
            {
                IsOffline = newOffline;
                StatusChanged?.Invoke(IsOffline);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _timer?.Dispose();
            await Task.CompletedTask;
        }
    }
}