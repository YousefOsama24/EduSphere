using EduSphere.Data;
using EduSphere.Models;
using EduSphere.Repositories;
using EduSphere.Repositories.Implementations;
using EduSphere.Repositories.Interfaces;
using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#endregion

#region Dependency Injection

builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));

builder.Services.AddScoped<ICourseRepository,CourseRepository>();

builder.Services.AddScoped<IRepository<Center>, Repository<Center>>();
builder.Services.AddScoped<IRepository<Student>,Repository<Student>>();
builder.Services.AddScoped<IRepository<Teacher>,Repository<Teacher>>();
builder.Services.AddScoped<IRepository<Course>,Repository<Course>>();
builder.Services.AddScoped<IRepository<Exam>,Repository<Exam>>();
builder.Services.AddScoped<IRepository<Group>,Repository<Group>>();
builder.Services.AddScoped<IRepository<Lecture>,Repository<Lecture>>();
builder.Services.AddScoped<IRepository<Notification>,Repository<Notification>>();
builder.Services.AddScoped<IRepository<Payment>,Repository<Payment>>();
builder.Services.AddScoped<IRepository<Question>,Repository<Question>>();
builder.Services.AddScoped<IRepository<Schedule>,Repository<Schedule>>();
builder.Services.AddScoped<IRepository<StudentAnswer>,Repository<StudentAnswer>>();
builder.Services.AddScoped<IRepository<Subscription>,Repository<Subscription>>();
builder.Services.AddScoped<IRepository<SubscriptionPlan>,Repository<SubscriptionPlan>>();
builder.Services.AddScoped<IRepository<Subscription>,Repository<Subscription>>();
builder.Services.AddScoped<IRepository<Choice>,Repository<Choice>>();
builder.Services.AddScoped<IRepository<Parent>, Repository<Parent>>();
builder.Services.AddScoped<IRepository<Enrollment>, Repository<Enrollment>>();
builder.Services.AddScoped<IRepository<AttendanceSession>, Repository<AttendanceSession>>();
builder.Services.AddScoped<IRepository<AttendanceRecord>, Repository<AttendanceRecord>>();
builder.Services.AddScoped<IRepository<Choice>, Repository<Choice>>();
builder.Services.AddScoped<IRepository<ParentStudent>, Repository<ParentStudent>>();
#endregion



#region MVC

builder.Services.AddControllersWithViews();

#endregion

var app = builder.Build();

#region Middleware

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

#endregion
#region DataSeeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await DbSeeder.SeedAsync(context, userManager, roleManager);
}
#endregion

#region Routing

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");

#endregion

app.Run();