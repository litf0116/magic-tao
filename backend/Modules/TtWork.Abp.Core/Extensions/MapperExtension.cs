using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TtWork.Abp.Applications.Dtos;

namespace TtWork.Abp.Extensions
{
    public static class MapperExtension
    {
        /// <summary>
        /// 将源对象映射到目标类型的新对象
        /// </summary>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <returns>映射后的对象</returns>
        public static TDestination MapTo<TDestination>(this object source) where TDestination : new()
        {
            // 如果源对象为空，返回目标类型的默认值
            if (source == null) return default(TDestination);

            // 创建目标类型的实例
            var destination = new TDestination();

            // 获取源对象的所有属性
            var sourceProperties = source.GetType().GetProperties();

            // 遍历源对象的所有属性
            foreach (var sourceProperty in sourceProperties)
            {
                try
                {
                    // 在目标类型中查找同名属性
                    var destinationProperty = typeof(TDestination).GetProperty(sourceProperty.Name);

                    // 如果目标类型存在同名属性且属性可写
                    if (destinationProperty != null && destinationProperty.CanWrite)
                    {
                        // 获取源属性的值
                        var value = sourceProperty.GetValue(source);

                        // 将值设置到目标对象的属性中
                        destinationProperty.SetValue(destination, value);
                    }
                }
                catch (Exception ex)
                {
                    // 单个属性映射失败时继续处理其他属性
                    //Console.WriteLine($"映射属性{sourceProperty.Name}时出错: {ex.Message}");
                }
            }

            return destination;
        }
    }

}
