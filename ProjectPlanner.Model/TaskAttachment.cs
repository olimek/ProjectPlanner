using System.ComponentModel.DataAnnotations;

namespace ProjectPlanner.Model
{
    public class TaskAttachment
    {
        [Key]
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty; // image, document, bom, etc.

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public string? ThumbnailPath { get; set; }

        public int SubTaskId { get; set; }

        public SubTask? SubTask { get; set; }
    }
}