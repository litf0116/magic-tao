using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using Abp.Dependency;
using Microsoft.Data.Sqlite;
using TtWork.SoMall.EntityFrameworkCore;
using TtWork.SoMall.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TTWork.Abp.Core.Identity;
using TTWork.Abp.Mall;
// using TTWork.Triggers;

namespace TtWork.SoMall.Tests.DependencyInjection
{
    public static class ServiceCollectionRegistrar
    {
        public static void Register(IIocManager iocManager)
        {
            RegisterIdentity(iocManager);

            var builder = new DbContextOptionsBuilder<AbpDbContext>();

            var inMemorySqlite = new SqliteConnection("Data Source=:memory:");

            builder
                .UseSqlite(inMemorySqlite)
                // .UseTriggers(triggerOptions =>
                // {
                //     //triggerOptions.AddTrigger<TTWork.Triggers.Gallery.GalleryEventTrigger>();
                //     triggerOptions.AddAssemblyTriggers(typeof(GalleryModule).Assembly);
                // })
                ;
            // .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryPossibleExceptionWithAggregateOperatorWarning))


            iocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<AbpDbContext>>()
                    .Instance(builder.Options)
                    .LifestyleSingleton()
            );

            inMemorySqlite.Open();

            new AbpDbContext(builder.Options).Database.EnsureCreated();
        }

        private static void RegisterIdentity(IIocManager iocManager)
        {
            var services = new ServiceCollection();

            IdentityRegistrar.Register(services);

            WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);
        }
    }
}