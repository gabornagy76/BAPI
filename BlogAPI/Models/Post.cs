using System;
using System.Collections.Generic;

namespace BlogAPI.Models;

public partial class Post
{
    public int Id { get; set; }

    public int BloggerId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Blogger Blogger { get; set; } = null!;
}
