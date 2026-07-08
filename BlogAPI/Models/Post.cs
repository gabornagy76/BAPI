using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogAPI.Models;

public partial class Post
{
    public int Id { get; set; }

    // Idegen kulcs: a névből találja ki:
    // Látja, hogy van egy Blogger típusú tulajdonság az osztályban.
    // Lát egy int BloggerId mezőt.
    // Ha máshogy neveznénk el, meg kellene adni neki, hogy:
    // [ForeignKey("Blogger")]    
    public int BloggerId { get; set; }

    // A = null!; (null-forgiving operátor) jelzi a fordítónak, hogy ez a mező nem lehet üres (NOT NULL) az adatbázisban, a program indulásakor pedig meg fogja kapni a megfelelő értéket.
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Ez egy úgynevezett Navigációs tulajdonság (Navigation Property). Nem egy egyszerű adatot tárol, hanem közvetlen elérést biztosít a bejegyzéshez tartozó Blogger objektumhoz.
    // A virtual kulcsszó lehetővé teszi az Entity Framework számára a Lazy Loading(lusta betöltés) használatát.Ez azt jelenti, hogy a blogger adatai csak akkor töltődnek be az adatbázisból, amikor a kódban ténylegesen megpróbálod elérni őket(pl.post.Blogger.UserName). 
    public virtual Blogger Blogger { get; set; } = null!;
}
