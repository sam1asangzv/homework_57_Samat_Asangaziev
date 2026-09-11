using System.ComponentModel.DataAnnotations;

namespace TodoListLab54.Models;

public sealed class TodoTask
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название задачи")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 120 символов")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Введите подробное описание задачи")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Описание должно быть от 5 до 2000 символов")]
    public string Description { get; set; } = "";

    [Required]
    public string CreatorId { get; set; } = "";

    [Required]
    public string CreatorName { get; set; } = "";

    public string? AssigneeId { get; set; }

    public string? AssigneeName { get; set; }

    [Required(ErrorMessage = "Выберите приоритет")]
    public TodoPriority Priority { get; set; }

    public TodoStatus Status { get; set; } = TodoStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ClosedAt { get; set; }
}
