using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Abp.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TtWork.Abp.AppManagement.Domain;
using TtWork.Abp.Core.Extensions;

namespace TtWork.Abp.AppManagement.EntityFrameworkCore {
    public static class AppManagementDbExtensions {
        public static void ConfigureAppManagement(this ModelBuilder builder) {
            builder.Entity<App>(b => {
                b.ToTable(AppManagementConsts.DbTablePrefix + "Apps", AppManagementConsts.DbSchema);

                b.Property(x => x.Name).IsRequired().HasMaxLength(AppManagementConsts.MaxNameLength);
                b.Property(x => x.ClientName).IsRequired().HasMaxLength(AppManagementConsts.MaxNameLength);

                b.Property(x => x.Value).HasConversion(
                    v => v.ToJsonString(false, false),
                    v => v.FromJsonStringExt<Dictionary<string, string>>(),
                    new ValueComparer<Dictionary<string, string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c)
                );

                b.Property(x => x.ProviderKey).HasMaxLength(AppManagementConsts.ProviderKeyLength);
                b.Property(x => x.ProviderName).HasMaxLength(AppManagementConsts.ProviderNameLength);
            });
        }
    }
}