using EduSphere.Data;
using EduSphere.Models;
using EduSphere.Repositories.Implementations;
using EduSphere.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EduSphere.Services.Interfaces;
using EduSphere.Services.Implementations;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Serilog;
using Serilog.Events;




var builder = WebApplication.CreateBuilder(args);





#region Serilog
Log.Logger = new LoggerConfiguration()

    .MinimumLevel.Information()

    .Enrich.FromLogContext()

    // Console
    .WriteTo.Console()

    // Authentication Logs
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly("@l = 'Information'")
        .WriteTo.File(
            "Logs/Authentication/auth-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30))

    // Error Logs
    .WriteTo.Logger(lc => lc
        .MinimumLevel.Error()
        .WriteTo.File(
            "Logs/Errors/error-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30))

    // Warning Logs
    .WriteTo.Logger(lc => lc
        .MinimumLevel.Warning()
        .WriteTo.File(
            "Logs/Warnings/warning-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30))

    // All Logs
    .WriteTo.File(
        "Logs/System/system-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")

    .CreateLogger();

builder.Host.UseSerilog();

#endregion


#region Database

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Identity

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#endregion
#region Services

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";

    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    options.LogoutPath = "/Identity/Account/Logout";

    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly",
        policy => policy.RequireRole("SuperAdmin"));

    options.AddPolicy("CenterManagerOnly",
        policy => policy.RequireRole("CenterManager"));

    options.AddPolicy("TeacherOnly",
        policy => policy.RequireRole("Teacher"));

    options.AddPolicy("StudentOnly",
        policy => policy.RequireRole("Student"));

    options.AddPolicy("ParentOnly",
        policy => policy.RequireRole("Parent"));
});
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

#endregion



#region Dependency Injection

builder.Services.AddScoped(typeof(Repository<>),
                           typeof(Repository<>));



#endregion


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
#region MVC
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();


#endregion

var app = builder.Build();
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

#region Middleware

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

#endregion
/*#region DataSeeder
using (var scope = app.Services.CreateScope())
{
var services = scope.ServiceProvider;

var context = services.GetRequiredService<ApplicationDbContext>();
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

await DbSeeder.SeedAsync(context, userManager, roleManager);
}
#endregion
*/
#region Routing

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Identity}/{controller=Account}/{action=Login}/{id?}");

#endregion

Log.Information("EduSphere Application Started.");
app.Run();
Log.CloseAndFlush();