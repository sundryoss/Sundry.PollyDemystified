using System.Net.Http.Headers;
using Polly;

namespace Sundry.PollyDemystified.Console.Interface;

public class TokenRetrievalHandler : DelegatingHandler
{
    private readonly ITokenService tokenService;
    private const string TokenRetrieval = nameof(TokenRetrieval);
    private const string TokenKey = nameof(TokenKey);
    public TokenRetrievalHandler(ITokenService service)
    {
        tokenService = service;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = request.GetPolicyExecutionContext();
        if (context.Count == 0)
        {
            context = new Context(TokenRetrieval, new Dictionary<string, object> { { TokenKey, await tokenService.GetToken() } });
            request.SetPolicyExecutionContext(context);
        }

        var token = (Token)context[TokenKey];

        if(token!= Token.Empty)
            request.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, token.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}