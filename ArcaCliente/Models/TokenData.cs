using System;

namespace ArcaCliente.Models
{
    public class TokenData
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
