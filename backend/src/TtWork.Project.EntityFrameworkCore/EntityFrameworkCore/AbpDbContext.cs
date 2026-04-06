using System;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Domain.Entities;
using Abp.Extensions;
using Abp.Json;
using Abp.Organizations;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TtWork.Abp.AppManagement.Domain;
using TtWork.Abp.AppManagement.EntityFrameworkCore;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Extensions;
using TtWork.Abp.Core.MultiTenancy;
using TtWork.Abp.DomianServices;
using TtWork.HttpClient.Weixin.Models;
using TtWork.Project.Domains;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.EntityFrameworkCore {
    public class AbpDbContext : AbpZeroDbContext<Tenant, Role, User, AbpDbContext> {
        public DbSet<WechatUserinfo> WechatUserinfo { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<CmsArticle> CmsArticles { get; set; }
        public DbSet<CmsCategory> CmsCategories { get; set; }
        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<BidHistory> BidHistories { get; set; }
        public DbSet<Announce> Announces { get; set; }
        public DbSet<BanedUser> BanedUsers { get; set; }
        public DbSet<SensitiveWord> SensitiveWords { get; set; }
        public DbSet<AuctionStartNotify> AuctionStartNotify { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<ChatEmoji> ChatEmoji { get; set; }
        public DbSet<ChatListDelete> ChatListDelete { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }


        public DbSet<UserDepositLog> UserDepositLog { get; set; }
        public DbSet<UserBalanceLog> UserBalanceLog { get; set; }
        public DbSet<UserAvatarHistory> UserAvatarHistories { get; set; }

        public DbSet<PayOrder> PayOrder { get; set; }
        public DbSet<WechatPaymentNotification> WechatPaymentNotification { get; set; }

        public DbSet<AppRelease> AppReleases { get; set; }


        #region TtWork.Abp.AppManagement

        public DbSet<App> Apps { get; set; }

        #endregion

        public AbpDbContext(DbContextOptions<AbpDbContext> options)
            : base(options) {
            base.SuppressAutoSetTenantId = false;
            // Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            builder.Entity<User>();
            builder.Entity<Edition>().HasMany<EditionFeatureSetting>().WithOne(b => b.Edition).IsRequired(false);
            builder.ConfigureAppManagement();


            builder.Entity<WechatUserinfo>().HasKey(ba => new { ba.openid, ba.TenantId });

            builder.Entity<UserFriend>().HasKey(ba => new { ba.UserId, ba.FriendId });
            builder.Entity<Message>(b => {
                b.HasKey(x => x.Id);
                // b.HasIndex(x => x.Time)
                //     .IsDescending();
                // b.HasIndex(x => x.Chan);
                b.HasIndex(ba => new { ba.Chan, ba.Time })
                    .IsDescending();
                b.HasIndex(ba => new { ba.From, ba.To, ba.Time })
                    .IsDescending();
            });
            builder.Entity<BanedUser>()
                .HasIndex(ba => new { ba.UserId, ba.EndTime, ba.Chan })
                .IsDescending();

            builder.Entity<ChatListDelete>()
                .HasKey(ba => new { ba.UserId, ba.ToUserId });
                

            builder.Entity<SensitiveWord>()
                .HasIndex(ba => ba.Content);

            builder.Entity<ChatEmoji>(b => {
                b.HasIndex(ba => new { ba.CreatorUserId, ba.CreationTime })
                    .IsDescending()
                    ;
            });

            builder.Entity<ChatChannel>(b => {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.ChannelId)
                    .IsUnique();
                b.HasIndex(ba => new { ba.ChannelType, ba.IsActive });
                b.HasIndex(ba => new { ba.User1Id, ba.User2Id });
                b.HasIndex(x => x.LastMessageTime)
                    .IsDescending();
            });


            builder.Entity<WechatPaymentNotification>(b => {
                b.HasKey(x => x.Id);
                // b.Property(x => x.RawData).HasConversion(
                //     v => v.ToJsonString(false, false),
                //     v => v.FromJsonStringExt<WeChatPayPaidEventModel>()
                // );
            });
        }


        protected override void CheckAndSetMayHaveTenantIdProperty(object entityAsObj) {
            if (SuppressAutoSetTenantId) {
                return;
            }

            //Only works for single tenant applications
            // if (MultiTenancyConfig?.IsEnabled ?? false)
            // {
            //     return;
            // }

            //Only set IMayHaveTenant entities
            if (!(entityAsObj is IMayHaveTenant)) {
                return;
            }

            var entity = entityAsObj.As<IMayHaveTenant>();

            //Don't set if it's already set
            if (entity.TenantId != null) {
                return;
            }

            entity.TenantId = GetCurrentTenantIdOrNull();
        }

        protected override void CheckAndSetMustHaveTenantIdProperty(object entityAsObj) {
            base.CheckAndSetMustHaveTenantIdProperty(entityAsObj);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
            configurationBuilder
                .Properties<Ulid>()
                .HaveConversion<UlidToStringConverter>()
                // .HaveConversion<UlidToBytesConverter>()
                ;
        }


        public class UlidToBytesConverter : ValueConverter<Ulid, byte[]> {
            private static readonly ConverterMappingHints defaultHints = new ConverterMappingHints(size: 16);

            public UlidToBytesConverter() : this(null) {
            }

            public UlidToBytesConverter(ConverterMappingHints mappingHints = null)
                : base(
                    convertToProviderExpression: x => x.ToByteArray(),
                    convertFromProviderExpression: x => new Ulid(x),
                    mappingHints: defaultHints.With(mappingHints)) {
            }
        }

        public class UlidToStringConverter(ConverterMappingHints mappingHints = null) :
            ValueConverter<Ulid, string>(
                convertToProviderExpression: x => x.ToString(),
                convertFromProviderExpression: x => ParseUlidSafely(x),
                mappingHints: defaultHints.With(mappingHints)
            ) {
            private static readonly ConverterMappingHints defaultHints = new(size: 26);

            public UlidToStringConverter() : this(null) {
            }

            private static Ulid ParseUlidSafely(string value) {
                if (string.IsNullOrEmpty(value))
                    return default;

                // Handle non-standard Ulid lengths by padding
                if (value.Length < 26)
                    value = value.PadLeft(26, '0');
                else if (value.Length > 26)
                    value = value.Substring(0, 26);

                try {
                    return Ulid.Parse(value);
                } catch {
                    return default;
                }
            }
        }
    }
}