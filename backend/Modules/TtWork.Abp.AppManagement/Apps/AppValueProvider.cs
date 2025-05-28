﻿using System.Collections.Generic;
using System.Threading.Tasks;
 using Abp.Dependency;

 namespace TtWork.Abp.AppManagement.Apps
{
    public abstract class AppValueProvider : IAppValueProvider, ITransientDependency
    {
        public abstract string Name { get; }

        protected IAppStore AppStore { get; }

        protected AppValueProvider(IAppStore appStore)
        {
            AppStore = appStore;
        }

        public abstract Task<Dictionary<string, string>> GetOrNullAsync(AppDefinition app);
    }
}