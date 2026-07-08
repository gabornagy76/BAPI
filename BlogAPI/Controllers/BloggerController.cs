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
        // HTTP PUT kérések kezelése, amit meglévő adatok teljes frissítésére (Update) használunk.
        [HttpPut]
        // A metódus két forrásból vár adatot:
        // 1. [FromQuery] int id: Az URL-ből (Query String) olvassa ki, hogy melyik bloggert akarjuk módosítani (?id=5).
        // 2. [FromBody] UpdateBloggerDTO updateBloggerDTO: A HTTP kérés törzséből (Body) kapja meg a frissítendő adatokat JSON formátumban.
        public async Task<ActionResult> UpdateBlogger([FromQuery] int id, [FromBody] UpdateBloggerDTO updateBloggerDTO)
        {
            try
            {
                // 1. LÉPÉS: Megkeressük a meglévő bloggert az adatbázisban az Id alapján.
                // A FirstOrDefaultAsync egy Lambda kifejezést (x => x.Id == id) vár feltételként.
                // MŰKÖDÉSE: Végignézi a táblát, és az ELSŐ olyan Blogger OBJEKTUMMAL tér vissza, amelyre igaz a feltétel. 
                // Ha nem talál semmit, akkor NULL értékkel tér vissza (tehát nem true/false értéket ad!).
                var blogger = await _blogContext.Bloggers.FirstOrDefaultAsync(x => x.Id == id);

                // Ha létezik, átmásoljuk a DTO-ban érkező ÚJ adatokat a meglévő, adatbázisból lekért objektumba.
                if (blogger != null)
                {
                    // Frissítsük az új értékekkel az objektumot.
                    blogger.UserName = updateBloggerDTO.UserName;
                    blogger.Password = updateBloggerDTO.Password;
                    blogger.Email = updateBloggerDTO.Email;

                    // await _blogContext.Bloggers.Update  // Az update függvénynek nincs async változata, ezért await nélkül csinálhatjuk meg. De nem fontos.
                    
                    // Kijelöljük az objektumot frissítésre az Entity Framework-ben.
                    // Igazából az '_blogContext.Bloggers.Update(blogger);' sor elhagyható lenne, 
                    // mert az EF 'Change Tracker'-e automatikusan észleli, hogy a fenti sorokban megváltoztattuk a 'blogger' tulajdonságait!
                    _blogContext.Bloggers.Update(blogger);
                    // a _blogContext.Bloggers.Update(blogger) parancs nem nyúl hozzá az adatbázishoz. Ez a parancs mindössze annyit csinál a számítógép memóriájában, hogy átállítja a blogger objektum belső állapotjelzőjét (State) Modified (Módosított) státuszra. Mivel ez csak egy minimális memóriaművelet, azonnal lefut, így nem igényel aszinkron (async/await) működést.

                    // Aszinkron módon elmentjük a módosításokat az adatbázisba. Ez generálja le az SQL UPDATE parancsot.
                    await _blogContext.SaveChangesAsync();

                    // 200 OK státusszal és a frissített objektummal térünk vissza.
                    return Ok(new
                    {
                        message = "Sikeres frissítés!",
                        result = blogger
                    }
                    );
                }

                // Ha a blogger null (azaz nem találtunk ilyen Id-jú rekordot), 404 Not Found státuszt küldünk vissza.
                return StatusCode(404, new
                {
                    message = "Nincs ilyen blogger!",
                    result = blogger            // Itt a result értéke null
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
        public async Task<ActionResult> DeleteBlogger(int id)
        {
            try
            {
                // Előbb lekérdezzük, ha van találat, akkor módosítunk:
                var blogger = await _blogContext.Bloggers.FindAsync(id);

                if (blogger != null)
                {
                    _blogContext.Bloggers.Remove(blogger);
                    await _blogContext.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Sikeres törlés!",
                        result = blogger
                    });

                }

                return StatusCode(404, new
                {
                    message = "Nincs ilyen blogger!",
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