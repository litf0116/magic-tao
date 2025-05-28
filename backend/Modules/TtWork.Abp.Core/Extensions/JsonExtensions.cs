using System;
using Abp.Json;
using Newtonsoft.Json;

namespace TtWork.Abp.Core.Extensions
{
    public static class JsonExtensionsExt
    {
        public static T FromJsonStringExt<T>(this string value)
        {
            try
            {
                return value.FromJsonString<T>(new JsonSerializerSettings());
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}