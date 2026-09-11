namespace TodoListLab54.Models;

public sealed class TodoTaskFilter
{
    public string? Title { get; set; }

    public string? DescriptionWords { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public TodoPriority? Priority { get; set; }

    public TodoStatus? Status { get; set; }

    public bool HasValues => !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(DescriptionWords) ||
        CreatedFrom.HasValue ||
        CreatedTo.HasValue ||
        Priority.HasValue ||
        Status.HasValue;
}
