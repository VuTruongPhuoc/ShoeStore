using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using ShoeStore.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ShoeStoreContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("ShoeStoreConnectStrings")));
builder.Services.AddDistributedMemoryCache();
//builder.Configuration.AddEnvironmentVariables("aspNetCore:environmentVariables:environmentVariable:value");
//builder.Services.AddDistributedRedisCache(option =>
//{
//    option.Configuration = "localhost:7162";
//    option.InstanceName = "SampleInstance";
//});
builder.Services.AddHttpClient();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromDays(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
//Add SendEmail
var sendmail = builder.Configuration.GetSection("SendEmail");
builder.Services.Configure<SendEmail>(sendmail);
builder.Services.AddSingleton<ISendEmail, SendEmailServices>();

//Add Export Excel
builder.Services.AddSingleton<IExcelHandler, ExcelHandler>();
//Add Authen
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Error/AccessDenied/";
        options.ReturnUrlParameter = "returnUrl";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        googleOptions.AccessDeniedPath = "/Error/AccessDenied";
       
        // Thiết lập đường dẫn Google chuyển hướng đến
        googleOptions.CallbackPath = "/signin-google";
    })
    .AddFacebook(facebookOptions => {
        // Đọc cấu hình
        IConfigurationSection facebookAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        facebookOptions.AccessDeniedPath = "/Error/AccessDenied";
        // Thiết lập đường dẫn Facebook chuyển hướng đến
        facebookOptions.CallbackPath = "/signin-facebook";
    });

builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
}
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/AccessDenied");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
        "admin", 
        "Admin/{controller}/{action}/{id?}",
        defaults: new { area = "Admin" }, 
        constraints: new { area = "Admin" });
app.MapAreaControllerRoute(
       name: "admin",
        areaName: "admin",
        pattern: "admin/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
