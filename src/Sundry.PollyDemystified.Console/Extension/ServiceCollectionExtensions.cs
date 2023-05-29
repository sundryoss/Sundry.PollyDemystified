namespace Sundry.PollyDemystified.Console.Extension;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Sundry.PollyDemystified.Console.Interface;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoService(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton<ITokenService, TokenService>();
        services.AddTransient<TokenRetrievalHandler>();

        var todoServiceOption = context.Configuration.GetSection(TodoServiceOption.ConfigKey).Get<TodoServiceOption>()!;
        services
                .AddHttpClient<ITodoService, TodoService>(client => client.BaseAddress = new Uri(todoServiceOption.BaseUrl))
                .AddPolicyHandler((provider,request)=> Policy.WrapAsync(PollyRetryPolicies.GetConstantBackofffPolicy(), PollyRetryPolicies.GetTokenRefresher(provider, request)))
                .AddHttpMessageHandler<TokenRetrievalHandler>();
        return services;
    }

    public static IServiceCollection AddAuth0Service(this IServiceCollection services, HostBuilderContext context)
    {
        var auth0Option = context.Configuration.GetSection(Auth0Option.ConfigKey).Get<Auth0Option>()!;

        services.AddSingleton(auth0Option);

        services
                .AddHttpClient<IAuth0Service, Auth0Service>(client =>
                {
                    client.BaseAddress = new Uri(auth0Option.BaseAddress);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                })
                .AddPolicyHandler(PollyRetryPolicies.GetConstantBackofffPolicy());
        return services;
    }
}