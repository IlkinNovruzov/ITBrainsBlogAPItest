﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ITBrainsBlogAPI.Models
{
    public class Like
    {
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
