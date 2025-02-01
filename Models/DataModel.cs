﻿using System.ComponentModel.DataAnnotations;

namespace AspBBS.Models
{
    public class DataModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string IP { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;

        public string Situation { get; set; } = string.Empty;

        public string UserID { get; set; } = string.Empty;
    }
}
