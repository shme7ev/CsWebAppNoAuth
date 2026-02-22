namespace WebAppNoAuth.IntegrationTests;

public static class TestConstants
{
    public const string TestUsername = "testuser";
    public const string AdminBaseUrl = "/Admin";
    public const string HomeBaseUrl = "/";
    
    public static class Endpoints
    {
        public const string AdminIndex = "/Admin";
        public const string AdminDashboard = "/Admin/Dashboard";
        public const string AdminLogin = "/Admin/Login";
        public const string AdminGenerateToken = "/Admin/GenerateToken";
        public const string HomeIndex = "/";
        public const string HomePrivacy = "/Home/Privacy";
    }
    
    public static class HttpStatusCodes
    {
        public const int Ok = 200;
        public const int Unauthorized = 401;
        public const int NotFound = 404;
    }
    
    public static class HttpHeaders
    {
        public const string Authorization = "Authorization";
        public const string WwwAuthenticate = "WWW-Authenticate";
        public const string ContentType = "Content-Type";
    }
    
    public static class ContentTypes
    {
        public const string Html = "text/html";
        public const string Json = "application/json";
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";
    }
    
    public static class Jwt
    {
        public const string BearerPrefix = "Bearer ";
        public const string InvalidToken = "invalid-token";
    }
}