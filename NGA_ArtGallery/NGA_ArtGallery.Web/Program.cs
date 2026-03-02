using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data;
using NGA_ArtGallery.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container, or smth, Idk (T~T)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
    x => x.MigrationsAssembly("NGA_ArtGallery.Web")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser<int>>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

app.Run();
