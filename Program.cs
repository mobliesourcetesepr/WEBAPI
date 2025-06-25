using AgentCreation.Data;
using AgentCreation.Hubs;
using AgentCreation.Providers;
using AgentCreation.Repositories;
using AgentCreation.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer(); // 🔹 Required for Swagger
builder.Services.AddSwaggerGen();   
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// ✅ Required for session
builder.Services.AddDataProtection();
builder.Services.AddDistributedMemoryCache();
// ✅ Register SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddMemoryCache();



// Optional: CORS for frontend connection
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowLocalhostFrontend");
// ✅ Map your SignalR hub endpoint
app.MapHub<NotificationHub>("/notificationhub");

app.UseWebSockets(); // Enable WebSockets

app.UseMiddleware<WebSocketDemo.Middlewares.WebSocketMiddleware>(); // Our WebSocket middleware

app.MapGet("/", () => "WebSocket API is running!");

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseSession(); 
app.UseAuthorization();
//app.UseStaticFiles(); // 👈 Enables serving HTML, JS, CSS
app.MapControllers();

app.Run();
