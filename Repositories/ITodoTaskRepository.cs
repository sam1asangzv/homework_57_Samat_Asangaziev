using TodoListLab54.Models;

namespace TodoListLab54.Repositories;

public interface ITodoTaskRepository
{
    IReadOnlyCollection<TodoTask> GetAll();

    TodoTask? GetById(int id);

    TodoTask Add(TodoTask task);

    bool Update(TodoTask task);

    bool Take(int id, string assigneeId, string assigneeName);

    bool Close(int id);

    bool Delete(int id);
}
