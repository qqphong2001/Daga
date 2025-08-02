using System.ComponentModel.DataAnnotations;

namespace webdaga.Models
{
    public class articlesModel
    {
        [Key]
        public int Id { get; set; } 
        public string ImgUrl { get; set; }
        public string VideoUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
