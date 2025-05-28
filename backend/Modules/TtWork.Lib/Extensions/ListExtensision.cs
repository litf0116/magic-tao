using System;
using System.Collections.Generic;

namespace TtWork.Lib.Extensions
{
    public static class ListExtensision
    {
        public static List<T> RandomSortList<T>(this IEnumerable<T> ListT)
        {
            var random = new Random();
            var newList = new List<T>();
            foreach (var item in ListT)
            {
                newList.Insert(random.Next(newList.Count + 1), item);
            }

            return newList;
        }
    }
}