// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.

// builder.Services.AddControllers();
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// var app = builder.Build();
// builder.Services.AddSingleton<MultiTenantAPI.Services.SessionService>();
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();

// app.Run();
// using Microsoft.OpenApi.Models;
// // using MultiTenantAPI.Services;
// // var builder = WebApplication.CreateBuilder(args);

// // // Add services to the container
// // builder.Services.AddControllers();
// // builder.Services.AddSingleton<MultiTenantAPI.Services.SessionService>();

// // // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// // builder.Services.AddEndpointsApiExplorer(); // Needed for Swagger
// // builder.Services.AddSwaggerGen(); // Also needed for Swagger

// // builder.Services.AddDistributedMemoryCache();
// // builder.Services.AddSession(options =>
// // {
// //     options.IdleTimeout = TimeSpan.FromHours(1);
// //     options.Cookie.HttpOnly = true;
// //     options.Cookie.IsEssential = true;
// // });

// // var app = builder.Build();

// // // Configure middleware
// // if (app.Environment.IsDevelopment())
// // {
// //     app.UseSwagger();
// //     app.UseSwaggerUI();
// // }

// // app.UseHttpsRedirection();
// // app.UseSession();
// // app.UseAuthorization();

// // app.MapControllers();
// // app.Run();
// using Microsoft.OpenApi.Models;
// using MultiTenantAPI.Services;
// using Microsoft.AspNetCore.DataProtection;
// using System.IO;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container
// builder.Services.AddControllers();
// builder.Services.AddDistributedMemoryCache();

// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromHours(1);
//     options.Cookie.HttpOnly = true;
//     options.Cookie.IsEssential = true;
// });

// var app = builder.Build();

// // Middleware pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();
// app.UseSession();         // Session must be before routing
// app.UseAuthorization();

// app.MapControllers();

// app.Run();



// using MultiTenantAPI.Middleware;
// using Microsoft.OpenApi.Models;
// using System.IO;
// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// // Enable Session support
// builder.Services.AddDistributedMemoryCache();
// builder.Services.AddSession(options =>
// {
//     options.Cookie.HttpOnly = true;
//     options.Cookie.IsEssential = true;
//     options.IdleTimeout = TimeSpan.FromHours(1);
// });

// var app = builder.Build();

// app.UseSwagger();
// app.UseSwaggerUI();

// app.UseHttpsRedirection();

// // Tenant extraction middleware (reads X-Tenant-ID header)
// app.UseTenantMiddleware();

// // Enable Session Middleware
// app.UseSession();

// // Authentication middleware using session tokens
// app.UseMiddleware<SessionAuthMiddleware>();

// app.UseAuthorization();

// app.MapControllers();

// app.Run();



using MultiTenantAPI.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.DataProtection;  // <-- Add this
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add persistent Data Protection key storage
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))  // Change this path as needed
    .SetApplicationName("MultiTenantAPI");  // Use a consistent app name

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
