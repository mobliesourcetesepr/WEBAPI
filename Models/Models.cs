namespace WEBAPI.Models
{
    public class User
    {
        public int Id { get; set; }  
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RequestModel
    {
        public string EncryptedPayload { get; set; }
    }

    public class SessionToken
    {
        public string Username { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class OtpRequest
    {
        public string Contact { get; set; } 
        public string Type { get; set; }  
    }


}