namespace Sundry.PollyDemystified.Console.Interface;

using System.Net.Http.Json;
using Sundry.PollyDemystified.Core.Models;

public interface ITodoService
{
    Task<IEnumerable<TodoItem>?> GetTodosAsync();
}


public class TodoService : ITodoService
{
    private readonly HttpClient _httpClient;

    public TodoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public Task<IEnumerable<TodoItem>?> GetTodosAsync()
    {
        return _httpClient.GetFromJsonAsync<IEnumerable<TodoItem>>("todo");
    }
}
