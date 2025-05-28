using System;
using System.Collections.Generic;

namespace TtWork.Abp.AppManagement.Apps
{
    [Serializable]
    public class AppCacheItem
    {
        public Dictionary<string, string> Value { get; set; }

        public AppCacheItem()
        {
        }

        public AppCacheItem(Dictionary<string, string> value)
        {
            Value = value;
        }

        public static string CalculateCacheKey(string name, string providerName, string providerKey)
        {
            return "pn:" + providerName + ",pk:" + providerKey + ",n:" + name;
        }
    }
}