using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManager.CoreMVC.Authorization;
using TaskManager.CoreMVC.Database;
using TaskManager.CoreMVC.Repositories;

var builder = WebApplication.CreateBuilder(args);

string conStr = builder.Configuration.GetConnectionString("MyConn");
builder.Services.AddDbContext<EmployeeTaskContext>(options => options.UseSqlServer(conStr));

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Add services to the container.
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSignalR();
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");



builder.Services.AddTransient<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddTransient<ITaskRepo, TaskRepo>();
builder.Services.AddTransient<AuthorizeRouteMiddleware>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Login/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();
app.UseMiddleware<AuthorizeRouteMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
