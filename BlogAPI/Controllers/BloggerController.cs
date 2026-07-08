using BlogAPI.Models;
using BlogAPI.Models.DTO;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
using System.Security.Policy;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [HttpGet]
        public async Task<ActionResult> GetAllBlogger()
        {
            try
            {
                return Ok(new
                {
                    message = "Sikeres lekérdezés",
                    result = await _blogContext.Bloggers.ToListAsync()
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

        // Id alapján lekérés
        // Ki kell egészíteni az URL-t, mivel ugyanarra az URL-re két GET metódust nem tudunk küldeni:
        [HttpGet("id")]        
        public async Task<ActionResult> GetBloggerById([FromQuery] int id)
        {
            try
            {
                var blogger = await _blogContext.Bloggers.FindAsync(id);
               
                if (blogger != null)
                {                    
                    return Ok(new
                    {
                        message = "Sikeres lekérdezés!",
                        result = blogger
                    }
                    );
                }

                return StatusCode(404, new
                {
                    message = "Sikertelen lekérdezés",
                    result = blogger        
                }
                );
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

        // Frissítő lekérdezés
        [HttpPut]
        // Két paramétert kell elküldenünk. Az azonosításhoz az Id-t, és a frissítendő adatokat.
        // Queryparaméterként megy az Id, az adatok pedig a törzsben.
        public async Task<ActionResult> UpdateBlogger()
        {

        }
    }
}