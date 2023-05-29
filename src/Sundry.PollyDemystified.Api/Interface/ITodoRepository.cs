using Sundry.PollyDemystified.Core.Models;

namespace Sundry.PollyDemystified.Api.Interface;

public interface ITodoRepository
{
    ValueTask<string> CreateAsync(string description);
    ValueTask<IEnumerable<TodoItem>> GetAllAsync();
    ValueTask<TodoItem> Get(string id);
    ValueTask Delete(string id);
    ValueTask Update(string id, bool completed);
}

public class TodoRepository : ITodoRepository
{
    private readonly Dictionary<string, TodoItem> items = new();

    public ValueTask<string> CreateAsync(string description)
    {
        var id = Guid.NewGuid().ToString("N");

        items.Add(id, new TodoItem(id, description, false));

        return ValueTask.FromResult(id);
    }

    public ValueTask Delete(string id)
    {
        items.Remove(id);
        return ValueTask.CompletedTask;
    }

    public ValueTask<TodoItem> Get(string id)
    {
        return ValueTask.FromResult(items[id]);
    }

    public ValueTask<IEnumerable<TodoItem>> GetAllAsync()
    {
        return ValueTask.FromResult(items.Values.AsEnumerable());
    }

    public ValueTask Update(string id, bool completed)
    {
        if (items.ContainsKey(id))
        {
            items[id] = items[id] with { Completed = completed };
        }
        return ValueTask.CompletedTask;
    }
}