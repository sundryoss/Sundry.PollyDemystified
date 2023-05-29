using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace Sundry.PollyDemystified.Console.Interface;
public record Token
{
    public static Token Empty => new();

    [JsonPropertyName("token_type")]
    public string Scheme { get; set; } = default!;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;

    [JsonPropertyName("expires_in")]
    public double ExpiresIn { get; set; } = default!;
}
public interface ITokenService
{
    ValueTask<Token> GetToken();
    Task<Token> RefreshToken();
}
public class TokenService : ITokenService
{
    private const string CacheKey = nameof(TokenService);

    private readonly IAuth0Service _auth0Service;
    private readonly IMemoryCache _memoryCache;
    public TokenService(IAuth0Service auth0Service, IMemoryCache memoryCache)
    {
        _auth0Service = auth0Service;
        _memoryCache = memoryCache;
    }
    public async ValueTask<Token> GetToken()
    {
        if (!_memoryCache.TryGetValue(CacheKey, out Token? cacheValue))
        {
            cacheValue = await RefreshToken();
        }
        return cacheValue!;
    }

    public async Task<Token> RefreshToken()
    {
        var token = await _auth0Service.GetTokenAsync();
        if (token != Token.Empty)
        {
            var expires_in = token.ExpiresIn>0?token.ExpiresIn-10:token.ExpiresIn;
            _memoryCache.Set(CacheKey, token, new MemoryCacheEntryOptions()
                                                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(expires_in)));
        }

        return token;
    }
}
