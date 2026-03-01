using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace TtWork.Project.EntityFrameworkCore {
    public static class AbpDbContextConfigurer {
        public static void Configure(DbContextOptionsBuilder<AbpDbContext> builder, string connectionString) {
            builder
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)), options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .ConfigureWarnings(warnings => { })

#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                ;
        }
            builder
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)))
                .ConfigureWarnings(warnings => { })

#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                ;
            ;
        }

        public static void Configure(DbContextOptionsBuilder<AbpDbContext> builder, DbConnection connectionString) {
            builder
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)), options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .ConfigureWarnings(warnings => { })

#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                ;
        }
            builder
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)))
                .ConfigureWarnings(warnings => { })
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                ;
            ;
        }
    }
}