namespace ArcaCliente.Models
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public int? RequestsLimit { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int? RequestsLimit { get; set; }
    }

    public class AuthResponse : ArcaBaseResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}
