using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace TtWork.Abp.AppManagement.Domain
{
    public class App : FullAuditedAggregateRoot<Guid>
    {
        [NotNull] public string Name { get; set; }

        [NotNull] public string ClientName { get; set; }


        // [Column(TypeName = "jsonb")] 
        public Dictionary<string, string> Value { get; set; }

        [CanBeNull] public virtual string ProviderName { get; protected set; }

        [CanBeNull] public virtual string ProviderKey { get; protected set; }

        protected App()
        {
        }

        public App(
            Guid id,
            [NotNull] string name,
            [NotNull] string clientName,
            [NotNull] Dictionary<string, string> value,
            [CanBeNull] string providerName = null,
            [CanBeNull] string providerKey = null)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(value, nameof(value));

            Id = id;
            Name = name;
            ClientName = clientName;
            Value = value;
            ProviderName = providerName;
            ProviderKey = providerKey;
        }
    }
}