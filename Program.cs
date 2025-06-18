using MultiTenantAPI.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.DataProtection;  // <-- Add this
using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Data;
using MultiTenantAPI.Services;
using System.IO;
using MultiTenantApi.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
builder.Services.AddSwaggerGen(options =>
{
    options.DocumentFilter<SwaggerIgnoreFilter>();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiTenantAPI", Version = "v1" });
});
var dbFlag = config["ActiveDatabase"];
Console.WriteLine($"ActiveDatabase: {dbFlag}");

if (dbFlag == "mysql")
{

    builder.Services.AddDbContext<UserDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

}
else if (dbFlag == "SqlServer")
{
    builder.Services.AddDbContext<UserDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
}
else
{
    throw new Exception("Invalid DbFlag value. Use 'mysql' or 'mssql'.");
}
builder.Services.AddSingleton<TokenService>();


builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();



builder.Services.AddHostedService<SessionCleanupService>();
// Enable Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(1);
});



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseSession();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<SessionAuthMiddleware>();    

app.UseAuthentication();


app.UseAuthorization();


app.UseAuthorization();

app.MapControllers();

app.Run();
