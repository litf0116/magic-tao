using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Newtonsoft.Json;

namespace TtWork.Abp.DomianServices {
    [Table("T_WechatUserinfo")]
    public class WechatUserinfo : Entity<string>, IMustHaveTenant, ICreationAudited {
        [JsonIgnore] [NotMapped] public override string Id => openid;

        [StringLength(32)] public string openid { get; set; }

        [StringLength(32)] public string unionid { get; set; }

        [StringLength(32)] public string nickname { get; set; }

        [StringLength(256)] public string headimgurl { get; set; }

        [StringLength(32)] public string city { get; set; }

        [StringLength(32)] public string province { get; set; }

        [StringLength(32)] public string country { get; set; }

        public int sex { get; set; }

        public int TenantId { get; set; }

        /// <summary>
        /// <exception cref="FromClient"></exception>
        /// </summary>
        public FromClient FromClient { get; set; }

        public WechatUserinfo(string openid, string unionid) {
            this.openid = openid;
            this.unionid = unionid;
        }

        public WechatUserinfo(string openid, string unionid, string nickname, string headimgurl, string city,
            string province, string country, int sex, FromClient fromClient = FromClient.WechatPublic) {
            this.openid = openid;
            this.unionid = unionid;
            this.nickname = nickname;
            this.headimgurl = headimgurl;
            this.city = city;
            this.province = province;
            this.country = country;
            this.sex = sex;
            this.FromClient = fromClient;
        }

        public void Update(string unionid, string nickname, string headimgurl, string city, string province,
            string country, int sex, FromClient fromClient = FromClient.WechatPublic) {
            this.unionid = unionid;
            this.nickname = nickname;
            this.headimgurl = headimgurl;
            this.city = city;
            this.province = province;
            this.country = country;
            this.sex = sex;
            FromClient = fromClient;
        }


        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }

        [NotMapped]
        public DateTime? LastModificationTime {
            get => this.CreationTime;
            set => CreationTime = value ?? DateTime.Now;
        }

        [NotMapped]
        public long? LastModifierUserId {
            get => this.CreatorUserId;
            set => CreatorUserId = value;
        }
    }

    public enum FromClient {
        Default = 0,
        WechatMini = 1,
        WechatPublic = 2
    }
}