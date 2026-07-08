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
        // Ha egy primitív a paraméter, automatikusan a query URL-ben kéri, de megadható
        // [FromQuery]: Explicit megmondja az API-nak, hogy az 'id' értékét az URL - ben lévő Query Stringből várjuk(pl. ? id = 5).
        // A metódus aszinkron (async Task<ActionResult>), így nem blokkolja a szervert az adatbázis-lekérdezés ideje alatt.
        public async Task<ActionResult> GetBloggerById([FromQuery] int id)
        {
            try
            {
                // Egy darab objektumot keresek, a kapott id alapján.
                // Az Entity Framework beépített 'FindAsync' metódusával megkeressük azt a rekordot,
                // amelynek az Elsődleges Kulcsa (Primary Key / Id) megegyezik a paraméterben kapott számmal. A FindAsync mindig elsődleges kulcsot keres. Többes kulcsnál a mezőazonosítókat vesszővel elválasztva kellene megadni.
                // Az 'await' megvárja, amíg az adatbázis visszatér az eredménnyel.

                var blogger = await _blogContext.Bloggers.FindAsync(id);

                // Megtalálta-e az id alapján:
                if (blogger != null)
                {
                    // Ha a blogger nem null (vagyis létezik ilyen Id-jú sor), 200 OK státusszal 
                    // és egy sikeres üzenettel visszaküldjük a megtalált blogger objektumot JSON formátumban.
                    return Ok(new
                    {
                        message = "Sikeres lekérdezés!",
                        result = blogger
                    }
                    );
                }

                // Ha a blogger null (vagyis nincs ilyen Id-jú rekord az adatbázisban),
                // egy 404 Not Found (Nem található) státuszkóddal térünk vissza a kliensnek.
                return StatusCode(404, new
                {
                    message = "Sikertelen lekérdezés",
                    result = blogger        // Itt a 'result' értéke értelemszerűen null lesz
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
    }
}