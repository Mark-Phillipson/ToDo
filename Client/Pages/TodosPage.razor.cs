using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using BlazorApp.Client.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Client.Services;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Client.Pages
{
	public partial class TodosPage
	{
		private List<ToDoList> todos = new();
		[Inject] public required ILocalStorageService LocalStorage { get; set; }
		[Inject] public required HttpClient Http { get; set; }
		[Inject] public required IJSRuntime JSRuntime{ get; set; }
		[Inject] public required IToastService toastService{ get; set; }
		[Inject] public required BlazorApp.Client.Services.OfflineStateService OfflineService { get; set; }

#pragma warning disable 414, 649, 169
		private string message = "";
		private bool _loadFailed = false;
		ElementReference SearchInput;
		private int textboxAreaRows = 2;
		private bool isOffline = false;
#pragma warning restore 414, 649, 169
		private string SearchTerm { get; set; } = string.Empty;
		public bool ShowCompleted { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            isOffline = OfflineService.IsOffline;
            OfflineService.StatusChanged += OfflineStatusChanged;
            await LoadData();
        }

        private void OfflineStatusChanged(bool offline)
        {
            isOffline = offline;
            if (offline)
                toastService.ShowWarning("You're now offline. Changes will be saved locally.");
            else
                toastService.ShowSuccess("You're back online!");
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            OfflineService.StatusChanged -= OfflineStatusChanged;
        }

		private async Task LoadData()
		{
			// First, try to load from local storage (this works offline)
			todos = await LocalStorage.GetItemAsync<List<ToDoList>>("todo") ?? new List<ToDoList>();
			
			// If no local data, try to load from sample data
			if (todos.Count == 0)
			{
				try
				{
					todos = await Http.GetFromJsonAsync<List<ToDoList>>("sample-data/todo.json") ?? new List<ToDoList>();
				}
				catch (Exception ex)
				{
					// If we're offline or sample data fails to load, just use an empty list
					Console.WriteLine($"Failed to load sample data: {ex.Message}");
					todos = new List<ToDoList>();
					if (!isOffline)
					{
						toastService.ShowWarning("Unable to load sample data. Starting with an empty list.");
					}
				}
			}
		}

		protected async Task SaveToDoAsync()
		{
			await LocalStorage.SetItemAsync<List<ToDoList>>("todo", todos);
			//message = $"Saved! {DateTime.Now.TimeOfDay.ToString("hh:nn")}";
			toastService.ShowSuccess("All to dos have been saved successfully!");
		}

		private string title { get; set; } = "Todo List";
		//usersJson = LoadJson(@"services\users.json");
		//users = JsonConvert.DeserializeObject<List<AspNetUser>>(usersJson);

		private string LoadJson(string fileName)
		{
			using (StreamReader reader = new StreamReader(fileName))
			{
				string json = reader.ReadToEnd();
				return json;
			}
		}
		private async Task AddToDoAsync()
		{
			ToDoList toDoList = new ToDoList { DateCreated = DateTime.Now.Date, Title = "", Description = "" };
			todos.Add(toDoList);
			await JSRuntime.InvokeVoidAsync("setFocus", $"{toDoList.Id}TitleInputBig");
		}
		private void DeleteToDo(ToDoList toDoList)
		{
			todos.Remove(toDoList);
		}

		private async Task ReloadTodosAsync()
		{
			await LoadData();
			if (isOffline)
			{
				toastService.ShowInfo("You're currently offline. Showing cached data.");
			}
			else
			{
				toastService.ShowSuccess("Data reloaded successfully!");
			}
		}
		async Task DownloadFileAsync()
		{
			//var text = todos.ToList().ToString();
			
			var text = JsonConvert.SerializeObject(todos.ToList());
			var bytes = System.Text.Encoding.UTF8.GetBytes(text);
			await FileUtility.SaveAs(JSRuntime, "todo.json", bytes);

		}
		private IList<string> files = new List<string>();

		async Task OnInputFileChange(InputFileChangeEventArgs e)
		{
			var  files = e.GetMultipleFiles(1);
			var file=files.FirstOrDefault();
			if (file == null) return;
			List<ToDoList>? todosImported;
			byte[] result;
			using (var reader = file.OpenReadStream())
			{
				try
				{
					result= new byte[reader.Length];
					await reader.ReadExactlyAsync(result, 0, (int)reader.Length);
				var text=System.Text.Encoding.ASCII.GetString(result);
				todosImported = JsonConvert.DeserializeObject<List<ToDoList>>(text);
				if (todosImported != null && todosImported.Count > 0)
					{
						foreach (var todo in todosImported)
						{
							todos.Add(todo);
						}
					}
				}
				catch (Exception exception)
				{
					message = exception.Message;
				}
			}
		}
		private async Task CopyTextToClipboard(ToDoList todo)
		{
			await JSRuntime.InvokeVoidAsync(
				"clipboardCopy.copyText", todo.Description);
			toastService.ShowSuccess( $"Copied Successfully at {DateTime.Now:hh:mm}");
		}
		private void IncreaseHeight()
		{
			textboxAreaRows++;

		}
		private void DecreaseHeight()
		{
			textboxAreaRows--;
		}
	}
}