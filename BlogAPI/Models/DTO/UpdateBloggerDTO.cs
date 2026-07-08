using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models.DTO
{
    public class UpdateBloggerDTO
    {
        
        public string? UserName { get; set; }
        
        public string? Password { get; set; }
        
        public string? Email { get; set; }
    }
}
