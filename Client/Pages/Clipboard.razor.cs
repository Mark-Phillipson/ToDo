using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazored.Toast.Services;
using BlazorApp.Client.Models;
using BlazorApp.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages
{
    public partial class ClipboardPage : ComponentBase, IDisposable
    {
        [Inject] public required ClipboardService ClipboardService { get; set; }
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Inject] public required IToastService toastService { get; set; }

        protected List<ClipboardItem> items = new();
        protected string SearchTerm { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            items = ClipboardService.GetAll();
            ClipboardService.OnChange += UpdateItems;
        }

        private void UpdateItems()
        {
            items = ClipboardService.GetAll();
            InvokeAsync(StateHasChanged);
        }

        protected IEnumerable<ClipboardItem> FilteredItems =>
            string.IsNullOrWhiteSpace(SearchTerm)
                ? items
                : items.Where(i => (i.Content ?? string.Empty).IndexOf(SearchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0);

        protected async Task CopyItemAsync(ClipboardItem item)
        {
            await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", item.Content);
            await ClipboardService.AddAsync(item.Content, "recopy");
            toastService.ShowSuccess($"Copied at {DateTime.Now:hh:mm}");
        }

        protected async Task TogglePin(ClipboardItem item) => await ClipboardService.TogglePinAsync(item.Id);
        protected async Task DeleteItem(ClipboardItem item) => await ClipboardService.RemoveAsync(item.Id);

        protected string FormatDateTime(DateTime dt)
        {
            try
            {
                var local = dt.ToLocalTime();
                var span = DateTime.Now - local;
                if (span.TotalSeconds < 60) return "just now";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
                if (span.TotalDays < 2) return $"Yesterday at {local:HH:mm}";
                return local.ToString("dd MMM yyyy HH:mm");
            }
            catch
            {
                return dt.ToString();
            }
        }

        public void Dispose()
        {
            ClipboardService.OnChange -= UpdateItems;
        }
    }
}
