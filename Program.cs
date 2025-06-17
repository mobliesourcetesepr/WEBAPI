using MultiTenantAPI.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.DataProtection;  // <-- Add this
using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Data;
using MultiTenantAPI.Services;
using System.IO;
using MultiTenantApi.Filters;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiTenantAPI", Version = "v1" });
});


// builder.Services.AddDbContext<UserDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

// builder.Services.AddDbContext<MyLogDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("MySqlLogConnection"),
//         ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlLogConnection")))
// );
//var redisConnection = config["Redis"];
var dbFlag = config["ActiveDatabase"];
Console.WriteLine($"ActiveDatabase: {dbFlag}");

// builder.Services.AddSingleton<RedisSessionService>();
// builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
//     builder.Configuration.GetConnectionString("Redis")));

if (dbFlag == "mysql")
{

    builder.Services.AddDbContext<UserDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

    builder.Services.AddDbContext<MyLogDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("MySqlLogConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlLogConnection")))
    );
}
else if (dbFlag == "SqlServer")
{
    builder.Services.AddDbContext<UserDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

    builder.Services.AddDbContext<MyLogDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"))
    );
}
else
{
    throw new Exception("Invalid DbFlag value. Use 'mysql' or 'mssql'.");
}
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = redisConnection;
// });

builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();



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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Tenant extraction middleware (reads X-Tenant-ID header)
app.UseTenantMiddleware();

// Enable Session Middleware
app.UseSession();

// Authentication middleware using session tokens
//app.UseMiddleware<SessionMiddleware>();
app.UseMiddleware<SessionAuthMiddleware>();

app.UseMiddleware<AuthTokenMiddleware>();     // Sets context from AES token

app.UseAuthentication();


app.UseAuthorization();

app.MapControllers();

app.Run();
