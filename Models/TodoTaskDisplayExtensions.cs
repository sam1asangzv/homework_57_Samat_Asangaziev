namespace TodoListLab54.Models;

public static class TodoTaskDisplayExtensions
{
    public static string ToDisplayName(this TodoPriority priority)
    {
        return priority switch
        {
            TodoPriority.High => "Высокий",
            TodoPriority.Medium => "Средний",
            TodoPriority.Low => "Низкий",
            _ => priority.ToString()
        };
    }

    public static string ToCssClass(this TodoPriority priority)
    {
        return priority switch
        {
            TodoPriority.High => "priority-high",
            TodoPriority.Medium => "priority-medium",
            TodoPriority.Low => "priority-low",
            _ => ""
        };
    }

    public static string ToDisplayName(this TodoStatus status)
    {
        return status switch
        {
            TodoStatus.New => "Новая",
            TodoStatus.Open => "Открыта",
            TodoStatus.Closed => "Закрыта",
            _ => status.ToString()
        };
    }
}
