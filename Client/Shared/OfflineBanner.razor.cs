using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading;

namespace BlazorApp.Client.Shared
{
    public partial class OfflineBanner : IDisposable
    {
        [Inject] public required BlazorApp.Client.Services.OfflineStateService OfflineService { get; set; }
        public bool IsOffline { get; set; }

        protected override void OnInitialized()
        {
            IsOffline = OfflineService.IsOffline;
            OfflineService.StatusChanged += HandleStatusChanged;
        }

        private void HandleStatusChanged(bool offline)
        {
            IsOffline = offline;
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            OfflineService.StatusChanged -= HandleStatusChanged;
        }
    }
}