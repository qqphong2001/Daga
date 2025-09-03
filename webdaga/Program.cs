using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using webdaga.Areas.admin.Models;
using webdaga.DbContext;
using webdaga.Helper;
using webdaga.Services;
using webdaga.SignalR;
using webdaga.SignalR.LiveStreamApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<StreamOptions>(builder.Configuration.GetSection("Stream"));
builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(

    builder.Configuration.GetConnectionString("defaulConnectSql")

    ));

builder.Services.AddDefaultIdentity<UserModel>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddHostedService<VideoCleanupService>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // khóa 15 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // số lần sai tối đa
    options.Lockout.AllowedForNewUsers = true; // áp dụng cho user mới
});

builder.Services.AddRazorPages();
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
                .AllowAnyOrigin()   // Cho ph?p t?t c? domain
                .AllowAnyHeader()   // Cho ph?p t?t c? header
                .AllowAnyMethod();  // Cho ph?p t?t c? HTTP method
        });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".ts"] = "video/mp2t";
provider.Mappings[".mpd"] = "application/dash+xml";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapHub<ChatHub>("/chathub");
app.MapHub<StreamHub>("/streamHub");
app.MapDefaultControllerRoute();
app.UseAuthentication();  
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
   name: "areas",
   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
 );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



await AdminAccountInitializer.EnsureAdminAccountAsync(app.Services);

app.Run();
