using Microsoft.EntityFrameworkCore;
using WebChatBot.Data; // Make sure this namespace matches your project structure
using Npgsql.EntityFrameworkCore.PostgreSQL; // Explicitly import PostgreSQL provider
using WebChatBot.Services;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SemanticKernelService>();

builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<TimeSlotGenerator>();

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder
            // If you want to allow specific origins (more secure):
            // .WithOrigins("https://example.com", "https://another-domain.com")
            // If you want to allow any origin (easier for development):
            .WithOrigins("http://localhost:3000")
            // Allow specific HTTP methods:
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            // Allow specific headers:
            .WithHeaders("Content-Type", "Authorization", "Accept")
            // Or allow any header:
            .AllowAnyHeader()
            .AllowCredentials()
            // Allow credentials (cookies, authorization headers):
            // .AllowCredentials()
            // Note: You cannot use AllowAnyOrigin() with AllowCredentials()
            // If you need credentials, you must specify origins
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowSpecificOrigins");
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
 
app.UseAuthorization();
app.UseSession();
// Remove or correct the MapStaticAssets call if it's not a valid method
// app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    // Remove the WithStaticAssets call if it's not a valid method
    // .WithStaticAssets();

// await DbInitializer.SeedData(app.Services);

app.Run();