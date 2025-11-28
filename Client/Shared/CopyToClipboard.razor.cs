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

namespace BlazorApp.Client.Shared
{
	public partial class CopyToClipboard
	{
		[Inject] public required ILocalStorageService LocalStorage { get; set; }
		[Inject] public required IJSRuntime JavascriptRuntime { get; set; }
		[Inject] public required HttpClient Http { get; set; }	[Parameter] public int Rows { get; set; }
		[Inject] public required BlazorApp.Client.Services.OfflineStateService OfflineService { get; set; }
	private List<ToDoList> todos = new();
	private bool isOffline = false;
	public string Text { get; set; } = string.Empty;
	public string Result { get; set; } = string.Empty;       
		private async Task CopyTextToClipboard()
		{
			await JavascriptRuntime.InvokeVoidAsync(
				"clipboardCopy.copyText", Text);
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
			// First, try to load from local storage (this works offline)
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
			if (string.IsNullOrEmpty(Text) || Text.Length < 1)
			{
				return;
			}
			await LoadData();
			if (isOffline)
			{
				// We remain fully functional offline; message string can reflect offline state
				Result = "Saving locally (offline).";
			}
			var titleLength = Text.Length;
			if (titleLength > 30)
			{
				titleLength = 30;
			}
			ToDoList toDoList = new ToDoList { DateCreated = DateTime.Now.Date, Title = $"{Text.Substring(0, titleLength).ToUpper()}..", Description = Text, Completed = false };
			todos.Add(toDoList);
			await LocalStorage.SetItemAsync<List<ToDoList>>("todo", todos);
		}

		protected override void OnInitialized()
		{
			isOffline = OfflineService.IsOffline;
			OfflineService.StatusChanged += s => { isOffline = s; InvokeAsync(StateHasChanged); };
		}

	}
}