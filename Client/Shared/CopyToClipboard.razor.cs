using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using BlazorApp.Client.Pages;
using BlazorApp.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage.StorageOptions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using BlazorApp.Client.Services;

namespace BlazorApp.Client.Shared
{
	public partial class CopyToClipboard
	{
		[Inject] public required ILocalStorageService LocalStorage { get; set; }
		[Inject] public required IJSRuntime JavascriptRuntime { get; set; }
		[Inject] public required HttpClient Http { get; set; }
		[Parameter] public int Rows { get; set; }
		[Inject] public required BlazorApp.Client.Services.OfflineStateService OfflineService { get; set; }
		[Inject] public required ClipboardService ClipboardService { get; set; }
	private List<ToDoList> todos = new();
	private bool isOffline = false;
	public string Text { get; set; } = string.Empty;
	public string Result { get; set; } = string.Empty;       
		private async Task CopyTextToClipboard()
		{
			if (string.IsNullOrWhiteSpace(Text)) return;
			await JavascriptRuntime.InvokeVoidAsync("clipboardCopy.copyText", Text);
			// Save into clipboard history
			await ClipboardService.AddAsync(Text, "dictation");
			Result = $"Copied Successfully at {DateTime.Now:hh:mm}";
		}
		private async Task ClearDictationAsync()
		{
			Text = string.Empty;
			Result = string.Empty;
			await JavascriptRuntime.InvokeVoidAsync("setFocus", "DictationBox");
		}

		private async Task LoadData()
		{
			// Keep legacy behavior for now (not used by new flow)
			todos = await LocalStorage.GetItemAsync<List<ToDoList>>("todo") ?? new List<ToDoList>();
			// If no local data, try to load from sample data
			if (todos.Count == 0 && !OfflineService.IsOffline)
			{
				try
				{
					todos = await Http.GetFromJsonAsync<List<ToDoList>>("sample-data/todo.json") ?? new List<ToDoList>();
				}
				catch
				{
					todos = new List<ToDoList>();
				}
			}
		}

		private async Task AddToDoAsync()
		{
			if (string.IsNullOrWhiteSpace(Text)) return;
			if (isOffline)
			{
				Result = "Saving locally (offline).";
			}
			await ClipboardService.AddAsync(Text, "dictation");
			Text = string.Empty;
		}

		protected override void OnInitialized()
		{
			isOffline = OfflineService.IsOffline;
			OfflineService.StatusChanged += s => { isOffline = s; InvokeAsync(StateHasChanged); };
		}

	}
}