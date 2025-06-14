using MultiTenantAPI.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.DataProtection;  // <-- Add this
using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Data;
using System.IO;
using MultiTenantApi.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add persistent Data Protection key storage
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))  // Change this path as needed
    .SetApplicationName("MultiTenantAPI");  // Use a consistent app name
builder.Services.AddHttpContextAccessor();

// builder.Services.AddDbContext<UserDbContext>((sp, options) =>
// {
//     var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
//     var tenantId = httpContext?.Items["TenantId"]?.ToString();
//     var tenantConfig = builder.Configuration.GetSection($"Tenants:{tenantId}");
//     var provider = tenantConfig["Provider"];
//     var connectionString = tenantConfig["ConnectionString"];

//     if (provider == "MySql")
//         options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
//     else
//         options.UseSqlServer(connectionString);
// });
builder.Services.AddSwaggerGen(options =>
{
    options.DocumentFilter<SwaggerIgnoreFilter>();
});


builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

// builder.Services.AddDbContext<MyLogDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("MySqlLogDb"),
//         ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlLogDb")))
// );

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
