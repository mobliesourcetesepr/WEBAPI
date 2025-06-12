using MultiTenantAPI.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.DataProtection;  // <-- Add this
using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add persistent Data Protection key storage
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))  // Change this path as needed
    .SetApplicationName("MultiTenantAPI");  // Use a consistent app name

    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));


// Enable Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(1);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Tenant extraction middleware (reads X-Tenant-ID header)
app.UseTenantMiddleware();

// Enable Session Middleware
app.UseSession();

// Authentication middleware using session tokens
app.UseMiddleware<SessionAuthMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
