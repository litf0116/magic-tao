﻿using System.Collections.Generic;

namespace TtWork.Abp.AppManagement.Apps
{
    public interface IAppValueProviderManager
    {
        List<IAppValueProvider> Providers { get; }
    }
}