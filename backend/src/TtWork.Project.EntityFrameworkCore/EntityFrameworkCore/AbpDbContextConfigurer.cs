using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace TtWork.Project.EntityFrameworkCore
{
    public static class AbpDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AbpDbContext> builder, string connectionString, IConfiguration configuration = null)
        {
            builder
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)));
            
            var enableSensitiveDataLogging = configuration?.GetValue<bool>("EnableSensitiveDataLogging", false) ?? false;
            if (enableSensitiveDataLogging)
            {
                builder.EnableSensitiveDataLogging();
            }
        }

        public static void Configure(DbContextOptionsBuilder<AbpDbContext> builder, DbConnection connectionString, IConfiguration configuration = null)
        {
            builder
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)));
            
            var enableSensitiveDataLogging = configuration?.GetValue<bool>("EnableSensitiveDataLogging", false) ?? false;
            if (enableSensitiveDataLogging)
            {
                builder.EnableSensitiveDataLogging();
            }
        }
    }
}