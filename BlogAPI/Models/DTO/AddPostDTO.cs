using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models.DTO
{
    public class AddPostDTO
    {
        // Bemásoljuk a 3 kellő adattagot, az idegen kulcsot kötelezővé tesszük:
        [Required]
        public int BloggerId { get; set; }
        
        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;
    }
}
