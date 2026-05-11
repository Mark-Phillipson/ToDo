using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.Models
{
    public class ClipboardItem
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateCaptured { get; set; } = DateTime.UtcNow;
        [Required]
        [StringLength(4000)]
        public required string Content { get; set; }
        public string? Source { get; set; }
        public bool Pinned { get; set; } = false;
        public string? Tags { get; set; }
        public int UseCount { get; set; } = 1;
    }
}
