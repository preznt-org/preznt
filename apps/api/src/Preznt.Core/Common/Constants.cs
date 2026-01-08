namespace Preznt.Core.Constants;

public static class Constants
{
    // OAuth Endpoints
    public const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    public const string TokenEndpoint = "https://github.com/login/oauth/access_token";
    
    // API
    public const string ApiBaseUrl = "https://api.github.com";
    
    // API Endpoints (relative to ApiBaseUrl)
    public const string UserEndpoint = ApiBaseUrl + "/user";
    public const string UserEmailsEndpoint = ApiBaseUrl + "/user/emails";
    public const string UserReposEndpoint = ApiBaseUrl + "/user/repos";
    
    // App Info (for User-Agent header)
    public const string AppName = "Preznt";
    public const string AppVersion = "1.0";
}