using Microsoft.AspNetCore.Identity;
using TodoListLab54.Models;

namespace TodoListLab54.Repositories;

public sealed class InMemoryIdentityStore :
    IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>,
    IRoleStore<IdentityRole>
{
    private readonly List<ApplicationUser> _users = [];
    private readonly List<IdentityRole> _roles = [];
    private readonly Dictionary<string, HashSet<string>> _userRoles = [];
    private readonly object _sync = new();

    public IQueryable<ApplicationUser> Users
    {
        get
        {
            lock (_sync)
            {
                return _users.Select(CloneUser).AsQueryable();
            }
        }
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            user.SecurityStamp ??= Guid.NewGuid().ToString();
            _users.Add(CloneUser(user));
        }

        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            int index = _users.FindIndex(item => item.Id == user.Id);

            if (index < 0)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Пользователь не найден." }));
            }

            _users[index] = CloneUser(user);
        }

        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _users.RemoveAll(item => item.Id == user.Id);
            _userRoles.Remove(user.Id);
        }

        return Task.FromResult(IdentityResult.Success);
    }

    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            ApplicationUser? user = _users.FirstOrDefault(item => item.Id == userId);
            return Task.FromResult(user is null ? null : CloneUser(user));
        }
    }

    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            ApplicationUser? user = _users.FirstOrDefault(item => item.NormalizedUserName == normalizedUserName);
            return Task.FromResult(user is null ? null : CloneUser(user));
        }
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            ApplicationUser? user = _users.FirstOrDefault(item => item.NormalizedEmail == normalizedEmail);
            return Task.FromResult(user is null ? null : CloneUser(user));
        }
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (!_userRoles.TryGetValue(user.Id, out HashSet<string>? roles))
            {
                roles = [];
                _userRoles[user.Id] = roles;
            }

            roles.Add(roleName);
        }

        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (_userRoles.TryGetValue(user.Id, out HashSet<string>? roles))
            {
                roles.Remove(roleName);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IList<string> roles = _userRoles.TryGetValue(user.Id, out HashSet<string>? names)
                ? names.ToList()
                : [];

            return Task.FromResult(roles);
        }
    }

    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            bool isInRole = _userRoles.TryGetValue(user.Id, out HashSet<string>? roles) && roles.Contains(roleName);
            return Task.FromResult(isInRole);
        }
    }

    public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            HashSet<string> userIds = _userRoles
                .Where(pair => pair.Value.Contains(roleName))
                .Select(pair => pair.Key)
                .ToHashSet();

            IList<ApplicationUser> users = _users
                .Where(user => userIds.Contains(user.Id))
                .Select(CloneUser)
                .ToList();

            return Task.FromResult(users);
        }
    }

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    Task<IdentityResult> IRoleStore<IdentityRole>.CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(role.Id))
            {
                role.Id = Guid.NewGuid().ToString();
            }

            _roles.Add(CloneRole(role));
        }

        return Task.FromResult(IdentityResult.Success);
    }

    Task<IdentityResult> IRoleStore<IdentityRole>.UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            int index = _roles.FindIndex(item => item.Id == role.Id);

            if (index < 0)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Роль не найдена." }));
            }

            _roles[index] = CloneRole(role);
        }

        return Task.FromResult(IdentityResult.Success);
    }

    Task<IdentityResult> IRoleStore<IdentityRole>.DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _roles.RemoveAll(item => item.Id == role.Id);
        }

        return Task.FromResult(IdentityResult.Success);
    }

    Task<string> IRoleStore<IdentityRole>.GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    Task<string?> IRoleStore<IdentityRole>.GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    Task IRoleStore<IdentityRole>.SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    Task<string?> IRoleStore<IdentityRole>.GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    Task IRoleStore<IdentityRole>.SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    Task<IdentityRole?> IRoleStore<IdentityRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IdentityRole? role = _roles.FirstOrDefault(item => item.Id == roleId);
            return Task.FromResult(role is null ? null : CloneRole(role));
        }
    }

    Task<IdentityRole?> IRoleStore<IdentityRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IdentityRole? role = _roles.FirstOrDefault(item => item.NormalizedName == normalizedRoleName);
            return Task.FromResult(role is null ? null : CloneRole(role));
        }
    }

    public void Dispose()
    {
    }

    private static ApplicationUser CloneUser(ApplicationUser user)
    {
        return new ApplicationUser
        {
            Id = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            EmailConfirmed = user.EmailConfirmed,
            PasswordHash = user.PasswordHash,
            SecurityStamp = user.SecurityStamp,
            ConcurrencyStamp = user.ConcurrencyStamp,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount
        };
    }

    private static IdentityRole CloneRole(IdentityRole role)
    {
        return new IdentityRole
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName,
            ConcurrencyStamp = role.ConcurrencyStamp
        };
    }
}
