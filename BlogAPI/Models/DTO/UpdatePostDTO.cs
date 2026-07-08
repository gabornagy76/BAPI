using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models.DTO
{
    public class UpdatePostDTO
    {
        [Required]
        public int BloggerId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;
    }
}
