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

        // Id alapján lekérés
        // Ki kell egészíteni az URL-t, mivel ugyanarra az URL-re két GET metódust nem tudunk küldeni:
        [HttpGet("id")]
        public async Task<ActionResult> GetPostById([FromQuery] int id)
        {
            try
            {
                var post = await _blogContext.Posts.FindAsync(id);

                if (post != null)
                {
                    return Ok(new
                    {
                        message = "Sikeres lekérdezés!",
                        result = post
                    }
                    );
                }

                return StatusCode(404, new
                {
                    message = "Sikertelen lekérdezés",
                    result = post
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
        // HTTP PUT kérések kezelése, amit meglévő adatok teljes frissítésére (Update) használunk.
        [HttpPut]
        // A metódus két forrásból vár adatot:
        // 1. [FromQuery] int id: Az URL-ből (Query String) olvassa ki, hogy melyik bloggert akarjuk módosítani (?id=5).
        // 2. [FromBody] UpdateBloggerDTO updateBloggerDTO: A HTTP kérés törzséből (Body) kapja meg a frissítendő adatokat JSON formátumban.
        public async Task<ActionResult> UpdatePost([FromQuery] int id, [FromBody] UpdatePostDTO updatePostDTO)
        {
            try
            {
                // 1. LÉPÉS: Megkeressük a meglévő bloggert az adatbázisban az Id alapján.
                // A FirstOrDefaultAsync egy Lambda kifejezést (x => x.Id == id) vár feltételként.
                // MŰKÖDÉSE: Végignézi a táblát, és az ELSŐ olyan Blogger OBJEKTUMMAL tér vissza, amelyre igaz a feltétel. 
                // Ha nem talál semmit, akkor NULL értékkel tér vissza (tehát nem true/false értéket ad!).
                var post = await _blogContext.Posts.FirstOrDefaultAsync(x => x.Id == id);

                // Ha létezik, átmásoljuk a DTO-ban érkező ÚJ adatokat a meglévő, adatbázisból lekért objektumba.
                if (post != null)
                {
                    // Frissítsük az új értékekkel az objektumot.
                    post.Title = updatePostDTO.Title;
                    post.Content = updatePostDTO.Content;
                    post.BloggerId = updatePostDTO.BloggerId;

                    // await _blogContext.Bloggers.Update  // Az update függvénynek nincs async változata, ezért await nélkül csinálhatjuk meg. De nem fontos.

                    // Kijelöljük az objektumot frissítésre az Entity Framework-ben.
                    // Igazából az '_blogContext.Bloggers.Update(blogger);' sor elhagyható lenne, 
                    // mert az EF 'Change Tracker'-e automatikusan észleli, hogy a fenti sorokban megváltoztattuk a 'blogger' tulajdonságait!
                    _blogContext.Posts.Update(post);
                    // a _blogContext.Bloggers.Update(blogger) parancs nem nyúl hozzá az adatbázishoz. Ez a parancs mindössze annyit csinál a számítógép memóriájában, hogy átállítja a blogger objektum belső állapotjelzőjét (State) Modified (Módosított) státuszra. Mivel ez csak egy minimális memóriaművelet, azonnal lefut, így nem igényel aszinkron (async/await) működést.

                    // Aszinkron módon elmentjük a módosításokat az adatbázisba. Ez generálja le az SQL UPDATE parancsot.
                    await _blogContext.SaveChangesAsync();

                    // 200 OK státusszal és a frissített objektummal térünk vissza.
                    return Ok(new
                    {
                        message = "Sikeres frissítés!",
                        result = post
                    }
                    );
                }

                // Ha a blogger null (azaz nem találtunk ilyen Id-jú rekordot), 404 Not Found státuszt küldünk vissza.
                return StatusCode(404, new
                {
                    message = "Nincs ilyen blogger!",
                    result = post            // Itt a result értéke null
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

        [HttpDelete]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                // Előbb lekérdezzük, ha van találat, akkor módosítunk:
                var post = await _blogContext.Posts.FindAsync(id);

                if (post != null)
                {
                    _blogContext.Posts.Remove(post);
                    await _blogContext.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Sikeres törlés!",
                        result = post
                    });

                }

                return StatusCode(404, new
                {
                    message = "Nincs ilyen poszt!",
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

    }
}
