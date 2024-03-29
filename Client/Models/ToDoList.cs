using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.Models
{
	public class ToDoList
	{
		[Required]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public DateTime DateCreated { get; set; }
		[Required]
		[StringLength(50)]
		public string Title { get; set; }
		[Required]
		[StringLength(600)]
		public string Description { get; set; }
		public string? PictureUrl { get; set; }
		public bool Completed { get; set; } = false;

	}
}
