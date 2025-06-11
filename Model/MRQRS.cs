public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RequestModel
{
    public string EncryptedPayload { get; set; }
}