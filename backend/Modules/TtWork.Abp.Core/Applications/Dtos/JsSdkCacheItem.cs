using System;

namespace TtWork.Abp.Applications.Dtos
{
    public class JsSdkCacheItem
    {
        public string Appid { get; set; }

        public string AppSecret { get; set; }

        public string Ticket { get; set; }

        public DateTimeOffset TimeCreated { get; set; }

        public DateTimeOffset TimeExpired { get; set; }
        
        public int ExpiresIn { get; set; } 
    }
}