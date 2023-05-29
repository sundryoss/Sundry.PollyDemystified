using System.Net.Http.Json;

namespace Sundry.PollyDemystified.Console.Interface;

public interface IAuth0Service
{
    Task<Token> GetTokenAsync();
}
public class Auth0Service : IAuth0Service
{
    private readonly HttpClient _httpClient;
    private readonly Auth0Option _settings;
    public Auth0Service(HttpClient client, Auth0Option settings)
    {
        _httpClient = client;
        _settings = settings;
    }
    public async Task<Token> GetTokenAsync()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["audience"] = _settings.Audience,
            ["grant_type"] = _settings.GrantType,
            ["scope"] = _settings.Scope
        });

            var result= await  _httpClient.PostAsync(_settings.TokenUrl, content);
            if (!result.IsSuccessStatusCode)
            {
               return Token.Empty;
            }
            var token = await result.Content.ReadFromJsonAsync<Token>();

            return token!;
    }
}