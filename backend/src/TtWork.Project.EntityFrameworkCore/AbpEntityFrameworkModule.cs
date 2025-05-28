using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using TtWork.Project.Core;
using TtWork.Project.EntityFrameworkCore;
using TtWork.Project.EntityFrameworkCore.Seed;

namespace TtWork.Project {
    [DependsOn(
            typeof(ProjectCoreModule),
            typeof(AbpZeroCoreEntityFrameworkCoreModule),
            typeof(AbpApplicationModule)
        )
    ]
    public class ProjectEntityFrameworkModule : AbpModule {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; } = true;

        public override void PreInitialize() {
            if (!SkipDbContextRegistration) {
                Configuration.Modules.AbpEfCore().AddDbContext<AbpDbContext>(options => {
                    if (options.ExistingConnection != null) {
                        AbpDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else {
                        AbpDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }
        }

        public override void Initialize() {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectEntityFrameworkModule).GetAssembly());
        }

        public override void PostInitialize() {
            if (!SkipDbSeed) {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}