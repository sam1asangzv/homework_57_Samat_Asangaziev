using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoListLab54.Models;
using TodoListLab54.Repositories;
using TodoListLab54.ViewModels;

namespace TodoListLab54.Controllers;

[Authorize]
public sealed class TasksController : Controller
{
    private const int PageSize = 10;
    private readonly ITodoTaskRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public TasksController(ITodoTaskRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    public IActionResult Index(
        string? title,
        string? descriptionWords,
        DateTime? createdFrom,
        DateTime? createdTo,
        TodoPriority? priority,
        TodoStatus? status,
        string sortBy = "created",
        string sortDirection = "desc",
        int page = 1)
    {
        TodoTaskFilter filter = new()
        {
            Title = title,
            DescriptionWords = descriptionWords,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            Priority = priority,
            Status = status
        };

        IEnumerable<TodoTask> tasks = ApplyUserAccessFilter(_repository.GetAll());
        tasks = ApplyFilter(tasks, filter);
        sortBy = NormalizeSortBy(sortBy);
        sortDirection = NormalizeSortDirection(sortDirection);
        tasks = ApplySorting(tasks, sortBy, sortDirection == "desc");

        int totalItems = tasks.Count();
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        return View(new TodoTaskIndexViewModel
        {
            Tasks = tasks.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
            Filter = filter,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Page = page,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    public IActionResult Create()
    {
        return View(new TodoTaskFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TodoTaskFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        string userId = CurrentUserId();
        string userName = CurrentUserName();
        TodoTask task = new()
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            CreatorId = userId,
            CreatorName = userName,
            Priority = viewModel.Priority!.Value
        };

        _repository.Add(task);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        TodoTask? task = _repository.GetById(id);

        if (task is null)
        {
            return NotFound();
        }

        if (!CanView(task))
        {
            return Forbid();
        }

        return View(task);
    }

    public IActionResult Edit(int id)
    {
        TodoTask? task = _repository.GetById(id);

        if (task is null)
        {
            return NotFound();
        }

        if (!CanEdit(task))
        {
            return Forbid();
        }

        return View(new TodoTaskFormViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(TodoTaskFormViewModel viewModel)
    {
        TodoTask? task = _repository.GetById(viewModel.Id);

        if (task is null)
        {
            return NotFound();
        }

        if (!CanEdit(task))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        _repository.Update(new TodoTask
        {
            Id = viewModel.Id,
            Title = viewModel.Title,
            Description = viewModel.Description,
            Priority = viewModel.Priority!.Value
        });

        return RedirectToAction(nameof(Details), new { id = viewModel.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Take(int id)
    {
        TodoTask? task = _repository.GetById(id);

        if (task is null)
        {
            return NotFound();
        }

        if (!CanTake(task))
        {
            TempData["ErrorMessage"] = "Эту задачу нельзя взять.";
            return RedirectToAction(nameof(Index));
        }

        _repository.Take(id, CurrentUserId(), CurrentUserName());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Close(int id)
    {
        TodoTask? task = _repository.GetById(id);

        if (task is null)
        {
            return NotFound();
        }

        if (!CanChangeStatus(task))
        {
            TempData["ErrorMessage"] = "Только исполнитель может закрыть задачу.";
            return RedirectToAction(nameof(Index));
        }

        _repository.Close(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        TodoTask? task = _repository.GetById(id);

        if (task is null)
        {
            return NotFound();
        }

        if (!CanDelete(task))
        {
            TempData["ErrorMessage"] = "Удалять задачу может только ее создатель.";
            return RedirectToAction(nameof(Index));
        }

        if (!_repository.Delete(id))
        {
            TempData["ErrorMessage"] = "Задачу не удалось удалить.";
        }

        return RedirectToAction(nameof(Index));
    }

    private IEnumerable<TodoTask> ApplyUserAccessFilter(IEnumerable<TodoTask> tasks)
    {
        if (User.IsInRole("admin"))
        {
            return tasks;
        }

        string userId = CurrentUserId();

        return tasks.Where(task =>
            task.CreatorId == userId ||
            task.AssigneeId == userId ||
            string.IsNullOrWhiteSpace(task.AssigneeId));
    }

    private static IEnumerable<TodoTask> ApplyFilter(IEnumerable<TodoTask> tasks, TodoTaskFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            tasks = tasks.Where(task => task.Title.Contains(filter.Title.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.DescriptionWords))
        {
            tasks = tasks.Where(task => task.Description.Contains(filter.DescriptionWords.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (filter.CreatedFrom.HasValue)
        {
            tasks = tasks.Where(task => task.CreatedAt.Date >= filter.CreatedFrom.Value.Date);
        }

        if (filter.CreatedTo.HasValue)
        {
            tasks = tasks.Where(task => task.CreatedAt.Date <= filter.CreatedTo.Value.Date);
        }

        if (filter.Priority.HasValue)
        {
            tasks = tasks.Where(task => task.Priority == filter.Priority.Value);
        }

        if (filter.Status.HasValue)
        {
            tasks = tasks.Where(task => task.Status == filter.Status.Value);
        }

        return tasks;
    }

    private static IEnumerable<TodoTask> ApplySorting(IEnumerable<TodoTask> tasks, string sortBy, bool descending)
    {
        return sortBy switch
        {
            "title" => descending ? tasks.OrderByDescending(task => task.Title) : tasks.OrderBy(task => task.Title),
            "priority" => descending ? tasks.OrderByDescending(task => task.Priority) : tasks.OrderBy(task => task.Priority),
            "status" => descending ? tasks.OrderByDescending(task => task.Status) : tasks.OrderBy(task => task.Status),
            _ => descending ? tasks.OrderByDescending(task => task.CreatedAt) : tasks.OrderBy(task => task.CreatedAt)
        };
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        return sortBy?.ToLower() switch
        {
            "title" => "title",
            "priority" => "priority",
            "status" => "status",
            _ => "created"
        };
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        return sortDirection?.ToLower() == "asc" ? "asc" : "desc";
    }

    private bool CanEdit(TodoTask task)
    {
        return User.IsInRole("admin") || task.CreatorId == CurrentUserId();
    }

    private bool CanView(TodoTask task)
    {
        return User.IsInRole("admin") ||
            task.CreatorId == CurrentUserId() ||
            task.AssigneeId == CurrentUserId() ||
            string.IsNullOrWhiteSpace(task.AssigneeId);
    }

    private bool CanDelete(TodoTask task)
    {
        return User.IsInRole("admin") || task.CreatorId == CurrentUserId();
    }

    private bool CanTake(TodoTask task)
    {
        return User.IsInRole("admin") ||
            (task.Status == TodoStatus.New && string.IsNullOrWhiteSpace(task.AssigneeId));
    }

    private bool CanChangeStatus(TodoTask task)
    {
        return User.IsInRole("admin") || task.AssigneeId == CurrentUserId();
    }

    private string CurrentUserId()
    {
        return _userManager.GetUserId(User) ?? "";
    }

    private string CurrentUserName()
    {
        return User.Identity?.Name ?? "Пользователь";
    }
}
