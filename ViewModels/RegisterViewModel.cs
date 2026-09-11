using System.ComponentModel.DataAnnotations;

namespace TodoListLab54.ViewModels;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Повторите пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = "";
}
