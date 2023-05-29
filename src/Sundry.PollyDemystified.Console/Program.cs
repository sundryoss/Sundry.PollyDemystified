namespace Sundry.PollyDemystified.Console;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sundry.PollyDemystified.Console.Extension;
using Sundry.PollyDemystified.Console.Interface;
using Sundry.PollyDemystified.Core.Models;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var todoService = host.Services.GetRequiredService<ITodoService>();
        await GetAllTodo(todoService);

        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
         .ConfigureServices((context, services) => services.AddMemoryCache()
                                                           .AddAuth0Service(context)
                                                           .AddTodoService(context))
        .ConfigureAppConfiguration((_, config) => config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true));

    public static async Task GetAllTodo(ITodoService todoService)
    {
        var todos = await todoService.GetTodosAsync();
        if (todos is null)
        {
            Console.WriteLine("Todos is null");
            return;
        }
        Console.WriteLine($"Todos: {todos.Count()}");
    }
}