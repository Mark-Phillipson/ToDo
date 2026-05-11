using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using BlazorApp.Client.Models;

namespace BlazorApp.Client.Services
{
    public class ClipboardService
    {
        private const string StorageKey = "clipboard.items";
        private const string MigrationKey = "clipboard.migration.v1";
        private readonly ILocalStorageService _localStorage;
        private List<ClipboardItem> _items = new();

        public event Action? OnChange;

        public ClipboardService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            _items = await _localStorage.GetItemAsync<List<ClipboardItem>>(StorageKey) ?? new List<ClipboardItem>();

            var migrated = await _localStorage.GetItemAsync<bool>(MigrationKey);
            if (!_items.Any() && !migrated)
            {
                // Try to migrate legacy todo items
                var legacy = await _localStorage.GetItemAsync<List<ToDoList>>("todo");
                if (legacy != null && legacy.Count > 0)
                {
                    foreach (var t in legacy)
                    {
                        var content = t.Description ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(content)) continue;
                        _items.Add(new ClipboardItem
                        {
                            Content = content,
                            DateCaptured = t.DateCreated == default ? DateTime.UtcNow : t.DateCreated,
                            Source = "migration"
                        });
                    }
                    await _localStorage.SetItemAsync(MigrationKey, true);
                    await SaveInternalAsync();
                }
            }

            // Deduplicate on load
            Deduplicate();
            NotifyStateChanged();
        }

        public List<ClipboardItem> GetAll()
        {
            return _items
                .OrderByDescending(i => i.Pinned)
                .ThenByDescending(i => i.DateCaptured)
                .ToList();
        }

        public async Task AddAsync(string content, string? source = null)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            content = content.Trim();
            var existing = _items.FirstOrDefault(i => string.Equals(i.Content?.Trim(), content, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.DateCaptured = DateTime.UtcNow;
                existing.UseCount++;
            }
            else
            {
                _items.Add(new ClipboardItem
                {
                    Content = content,
                    DateCaptured = DateTime.UtcNow,
                    Source = source
                });
            }
            await SaveInternalAsync();
            NotifyStateChanged();
        }

        public async Task TogglePinAsync(string id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.Pinned = !item.Pinned;
                await SaveInternalAsync();
                NotifyStateChanged();
            }
        }

        public async Task RemoveAsync(string id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _items.Remove(item);
                await SaveInternalAsync();
                NotifyStateChanged();
            }
        }

        private void Deduplicate()
        {
            _items = _items
                .GroupBy(i => (i.Content ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(i => i.DateCaptured).First())
                .ToList();
        }

        private async Task SaveInternalAsync()
        {
            await _localStorage.SetItemAsync(StorageKey, _items);
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
