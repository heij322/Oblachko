using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Oblachko.Data;
using Oblachko.Services;

var builder = WebApplication.CreateBuilder(args);

var databaseFile = Directory.Exists("/data") ? "/data/oblachko.db" : "oblachko.db";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=" + databaseFile));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<RussianIdentityErrors>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

if (Directory.Exists("/data"))
{
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/data/keys"));
}

builder.Services.AddSingleton<UploadStorage>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

var russian = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = russian;
CultureInfo.DefaultThreadCurrentUICulture = russian;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Status/500");
}

app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(app.Services.GetRequiredService<UploadStorage>().Folder),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();

public partial class Program
{
}
