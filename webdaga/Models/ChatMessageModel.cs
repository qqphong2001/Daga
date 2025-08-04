using System.ComponentModel.DataAnnotations;

namespace webdaga.Models
{
    public class ChatMessageModel
    {
        [Key]
        public int Id { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
