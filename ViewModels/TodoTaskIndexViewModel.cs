using TodoListLab54.Models;

namespace TodoListLab54.ViewModels;

public sealed class TodoTaskIndexViewModel
{
    public IReadOnlyCollection<TodoTask> Tasks { get; set; } = [];

    public TodoTaskFilter Filter { get; set; } = new();

    public string SortBy { get; set; } = "created";

    public string SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public string NextDirectionFor(string column)
    {
        return SortBy == column && SortDirection == "asc" ? "desc" : "asc";
    }
}
