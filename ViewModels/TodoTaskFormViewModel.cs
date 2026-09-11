using System.ComponentModel.DataAnnotations;
using TodoListLab54.Models;

namespace TodoListLab54.ViewModels;

public sealed class TodoTaskFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название задачи")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 120 символов")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Выберите приоритет")]
    public TodoPriority? Priority { get; set; }

    [Required(ErrorMessage = "Введите подробное описание задачи")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Описание должно быть от 5 до 2000 символов")]
    public string Description { get; set; } = "";
}
