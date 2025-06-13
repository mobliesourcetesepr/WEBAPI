// File: Helpers/Logger.cs
using Microsoft.Extensions.DependencyInjection;
using MultiTenantAPI.Data;
using MultiTenantAPI.Models;
using System;

namespace MultiTenantAPI.Helpers
{
    public static class Logger
    {
        public static void LogData(IServiceProvider services,
                                    string logType,
                                    string pageName,
                                    string functionName,
                                    string Message)
        {
            try
            {
                using var scope = services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

                var log = new LogDetail
                {
                    LogType = logType,
                    PageName = pageName,
                    FunctionName = functionName,
                    LogData = Message,
                    CreatedOn = DateTime.Now
                };

                dbContext.LogDetails.Add(log);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging failed: " + ex.Message);
            }
        }
        
    //      public static void LogAudit(IServiceProvider services, Admin admin, string action, string changedBy, string changeDetails = "")
    // {
    //     using var scope = services.CreateScope();
    //     var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    //     var audit = new AdminAudit
    //     {
    //         TenantId = admin.TenantId,
    //         Username = admin.Username,
    //         Email = admin.Email,
    //         Action = action,
    //         ChangedBy = changedBy,
    //         ChangedAt = DateTime.UtcNow,
    //         ChangeDetails = changeDetails
    //     };

    //     dbContext.AdminAudits.Add(audit);
    //     dbContext.SaveChanges();
    // }
    }
}