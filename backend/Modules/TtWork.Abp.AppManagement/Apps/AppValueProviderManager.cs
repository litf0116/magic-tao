﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Abp.Dependency;
 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace TtWork.Abp.AppManagement.Apps
{
    public class AppValueProviderManager : IAppValueProviderManager, ISingletonDependency
    {
        public List<IAppValueProvider> Providers => _lazyProviders.Value;
        protected AppOptions Options { get; }
        private readonly Lazy<List<IAppValueProvider>> _lazyProviders;

        public AppValueProviderManager(
            IServiceProvider serviceProvider,
            AppOptions options)
        {
            Options = options;

            _lazyProviders = new Lazy<List<IAppValueProvider>>(
                () => Options
                    .ValueProviders
                    .Select(type => serviceProvider.GetRequiredService(type) as IAppValueProvider)
                    .ToList(),
                true
            );
        }
    }
}