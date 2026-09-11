using TodoListLab54.Models;

namespace TodoListLab54.Repositories;

public sealed class InMemoryTodoTaskRepository : ITodoTaskRepository
{
    private readonly List<TodoTask> _tasks =
    [
        new TodoTask
        {
            Id = 1,
            Title = "Подготовить макет страницы",
            Description = "Сверстать главную таблицу списка задач и проверить отображение кнопок действий.",
            CreatorId = "seed-admin",
            CreatorName = "admin@example.com",
            Priority = TodoPriority.High,
            Status = TodoStatus.New,
            CreatedAt = DateTime.Now.AddDays(-3)
        },
        new TodoTask
        {
            Id = 2,
            Title = "Проверить фильтрацию",
            Description = "Добавить поля фильтрации по названию, описанию, приоритету, статусу и датам.",
            CreatorId = "seed-admin",
            CreatorName = "admin@example.com",
            AssigneeId = "seed-user",
            AssigneeName = "user@example.com",
            Priority = TodoPriority.Medium,
            Status = TodoStatus.Open,
            CreatedAt = DateTime.Now.AddDays(-2)
        },
        new TodoTask
        {
            Id = 3,
            Title = "Сдать лабораторную",
            Description = "Проверить сборку проекта и подготовить работу к отправке преподавателю.",
            CreatorId = "seed-user",
            CreatorName = "user@example.com",
            AssigneeId = "seed-user",
            AssigneeName = "user@example.com",
            Priority = TodoPriority.Low,
            Status = TodoStatus.Closed,
            CreatedAt = DateTime.Now.AddDays(-1),
            ClosedAt = DateTime.Now
        }
    ];

    private int _nextId = 4;

    public IReadOnlyCollection<TodoTask> GetAll()
    {
        return _tasks;
    }

    public TodoTask? GetById(int id)
    {
        return _tasks.FirstOrDefault(task => task.Id == id);
    }

    public TodoTask Add(TodoTask task)
    {
        task.Id = _nextId++;
        task.Title = task.Title.Trim();
        task.Description = task.Description.Trim();
        task.Status = TodoStatus.New;
        task.AssigneeId = null;
        task.AssigneeName = null;
        task.CreatedAt = DateTime.Now;
        task.ClosedAt = null;

        _tasks.Add(task);
        return task;
    }

    public bool Update(TodoTask task)
    {
        TodoTask? existingTask = GetById(task.Id);

        if (existingTask is null)
        {
            return false;
        }

        existingTask.Title = task.Title.Trim();
        existingTask.Description = task.Description.Trim();
        existingTask.Priority = task.Priority;
        return true;
    }

    public bool Take(int id, string assigneeId, string assigneeName)
    {
        TodoTask? task = GetById(id);

        if (task is null || task.Status != TodoStatus.New || !string.IsNullOrWhiteSpace(task.AssigneeId))
        {
            return false;
        }

        task.AssigneeId = assigneeId;
        task.AssigneeName = assigneeName;
        task.Status = TodoStatus.Open;
        task.ClosedAt = null;
        return true;
    }

    public bool Close(int id)
    {
        TodoTask? task = GetById(id);

        if (task is null || task.Status != TodoStatus.Open)
        {
            return false;
        }

        task.Status = TodoStatus.Closed;
        task.ClosedAt = DateTime.Now;
        return true;
    }

    public bool Delete(int id)
    {
        TodoTask? task = GetById(id);

        if (task is null)
        {
            return false;
        }

        return _tasks.Remove(task);
    }
}
