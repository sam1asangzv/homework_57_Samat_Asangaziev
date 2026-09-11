using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using TodoListLab54.Models;
using TodoListLab54.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

string dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionPath);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITodoTaskRepository, InMemoryTodoTaskRepository>();
builder.Services.AddSingleton<InMemoryIdentityStore>();
builder.Services.AddSingleton<IUserStore<ApplicationUser>>(provider => provider.GetRequiredService<InMemoryIdentityStore>());
builder.Services.AddSingleton<IRoleStore<IdentityRole>>(provider => provider.GetRequiredService<InMemoryIdentityStore>());
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}")
    .WithStaticAssets();

SeedIdentity(app.Services);

app.Run();

static void SeedIdentity(IServiceProvider services)
{
    using IServiceScope scope = services.CreateScope();
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    EnsureRole(roleManager, "user");
    EnsureRole(roleManager, "admin");

    EnsureUser(userManager, "seed-admin", "admin@example.com", "Admin123", "admin");
    EnsureUser(userManager, "seed-user", "user@example.com", "User123", "user");
}

static void EnsureRole(RoleManager<IdentityRole> roleManager, string roleName)
{
    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
    {
        roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
    }
}

static void EnsureUser(UserManager<ApplicationUser> userManager, string id, string email, string password, string role)
{
    ApplicationUser? user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

    if (user is null)
    {
        user = new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        userManager.CreateAsync(user, password).GetAwaiter().GetResult();
    }

    if (!userManager.IsInRoleAsync(user, role).GetAwaiter().GetResult())
    {
        userManager.AddToRoleAsync(user, role).GetAwaiter().GetResult();
    }
}
