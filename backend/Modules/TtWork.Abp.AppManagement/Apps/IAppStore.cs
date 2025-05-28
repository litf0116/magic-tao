﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TtWork.Abp.AppManagement.Apps
{
    public interface IAppStore
    {
        Task<Dictionary<string, string>> GetOrNullAsync(
            [NotNull] string name,
            [CanBeNull] string providerName,
            [CanBeNull] string providerKey
        );
    }
}