using BlogAPI.Models;
using BlogAPI.Models.DTO;
using Google.Protobuf.WellKnownTypes;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mysqlx.Crud;

namespace BlogAPI.Controllers
{    
    [Route("blogger")]
    [ApiController]
    public class BloggerController : ControllerBase
    {
        private readonly BlogContext _blogContext;
        
        public BloggerController(BlogContext blogContext)
        {
            _blogContext = blogContext;
        }
        
        [HttpPost]
        public async Task<ActionResult> AddNewBlogger(AddBloggerDTO addBloggerDTO)
        {
            try
            {                
                var blogger = new Blogger()
                {
                    UserName = addBloggerDTO.UserName,
                    Password = addBloggerDTO.Password,
                    // Éles környezetben ezt hashelni illik!
                    Email = addBloggerDTO.Email
                };
                                
                if (blogger != null)
                {                    
                    await _blogContext.Bloggers.AddAsync(blogger);

                    await _blogContext.SaveChangesAsync();

                    return StatusCode(201, new
                    {
                        message = "Sikeres felvétel",
                        result = blogger
                    });
                }

                return StatusCode(404, new
                {
                    message = "Sikertelen felvitel",
                    result = blogger
                });
            }
            catch (Exception ex)
            {
                var realMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                
                return StatusCode(400, new
                {
                message = realMessage
                });
            }
        }
    }
}