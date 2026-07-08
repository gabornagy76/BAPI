using BlogAPI.Models;
using BlogAPI.Models.DTO;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlogAPI.Controllers
{    
    [Route("post")]
    [ApiController]
    public class PostController : ControllerBase
    {       
        private readonly BlogContext _blogContext;

        public PostController(BlogContext blogContext)
        {
            _blogContext = blogContext;
        }

        [HttpPost]
        public async Task<ActionResult> AddNewPost(AddPostDTO addPostDTO)
        {
            try
            {
                var post = new Post
                {
                    Title = addPostDTO.Title,
                    Content = addPostDTO.Content,
                    BloggerId = addPostDTO.BloggerId
                };

                // Megnézzük, hogy a post objektum létezik-e.
                if (post != null)
                {                   
                    await _blogContext.Posts.AddAsync(post);
                    await _blogContext.SaveChangesAsync();

                    return StatusCode(201, new
                    {
                        message = "Sikeres felvitel!",
                        result = post
                    });
                }

                return StatusCode(404, new
                {
                    message = "Sikertelen felvitel!",
                    result = post
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

        [HttpGet]
        public async Task<ActionResult> GetAllPost()
        {
            try
            {
                return Ok(new
                {
                    message = "Sikeres lekérdezés",
                    result = await _blogContext.Posts.ToListAsync()
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
