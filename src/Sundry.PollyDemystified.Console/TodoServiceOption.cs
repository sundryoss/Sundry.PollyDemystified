public class TodoServiceOption
{
    public const string ConfigKey = "TodoServiceSettings";
    public string BaseUrl { get; set; } = default!;
}

public class Auth0Option
{
    public const string ConfigKey = "Auth0Settings";
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string GrantType { get; set; } = default!;
    public string Scope { get; set; } = default!;
    public string BaseAddress { get; set; } = default!;
    public string TokenUrl { get; set; } = default!;
    
}