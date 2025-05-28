using System;
using Newtonsoft.Json;

namespace TtWork.Lib
{
    public static class JsonExt
    {
        public static T TryConvert<T>(this string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}