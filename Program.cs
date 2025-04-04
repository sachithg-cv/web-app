using Microsoft.EntityFrameworkCore;
using WebChatBot.Data; // Make sure this namespace matches your project structure
using Npgsql.EntityFrameworkCore.PostgreSQL; // Explicitly import PostgreSQL provider
using WebChatBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<SemanticKernelService>();

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Remove or correct the MapStaticAssets call if it's not a valid method
// app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    // Remove the WithStaticAssets call if it's not a valid method
    // .WithStaticAssets();

app.Run();