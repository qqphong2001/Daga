using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using webdaga.Areas.admin.Models;

public static class AdminAccountInitializer
{
    public static async Task EnsureAdminAccountAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

        string adminEmail = "Admin@gmail.com";
        string adminPassword = "AdminGaChoi@111";

        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser == null)
        {
            var adminUser = new UserModel
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Không thể tạo tài khoản admin: {string.Join(", ", result.Errors)}");
            }
        }
    }
}

// Trong Program.cs, ngay trước app.Run():
// await AdminAccountInitializer.EnsureAdminAccountAsync(app.Services);
