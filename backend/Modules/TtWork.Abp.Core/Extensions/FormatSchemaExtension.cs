using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Localization;
using Newtonsoft.Json.Linq;

namespace TtWork.Abp.Core.Extensions {
    public static class FormatSchemaExtension {
        // public static JSchema GetSchema<T>(this Type modal, bool useStringEnum = true) where T : ICanSchema {
        //     var generator = new JSchemaGenerator();
        //     generator.GenerationProviders.Add(new FormatSchemaProvider());
        //     if (useStringEnum)
        //         generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        //
        //     //abp api首字母小写
        //     generator.ContractResolver = new CamelCasePropertyNamesContractResolver();
        //
        //     var schema = generator.Generate(typeof(T), true);
        //
        //     schema.Properties["id"].ReadOnly = true;
        //     schema.Properties["id"].Default = 0;
        //
        //     return schema;
        // }

        public static JArray GetSelection<T>(this IList<T> list, string type, string title, string labelFormat,
            string[] labelKeys, string valueKey, int? colspan = null, bool justEnum = true, bool withParentId = false,
            string parentIdKey = null) {
            var selection = new JArray();
            foreach (var ct in list) {
                var args = new object[labelKeys.Length];
                for (var i = 0; i < labelKeys.Length; i++) {
                    if (labelKeys[i].IndexOf(".", StringComparison.Ordinal) > -1) {
                        var a = typeof(T).GetProperty(labelKeys[i].Split('.')[0])?.GetValue(ct, null);
                        var pType = typeof(T).GetProperty(labelKeys[i].Split('.')[0])?.PropertyType;
                        var p2 = labelKeys[i].Split('.')[1];
                        var b = pType != null ? pType.GetProperty(p2)?.GetValue(a, null) : "";
                        args[i] = b;
                    }
                    else {
                        var label = typeof(T).GetProperty(labelKeys[i])?.GetValue(ct, null);
                        args[i] = label;
                    }
                }


                if (type == "string") {
                    var o = new JObject {
                        { "label", string.Format(labelFormat, args) },
                        { "value", typeof(T).GetProperty(valueKey)?.GetValue(ct, null)?.ToString() }
                    };
                    if (withParentId && !parentIdKey.IsNullOrEmpty()) {
                        o["parentId"] =
                            Convert.ToInt32(typeof(T).GetProperty(parentIdKey)?.GetValue(ct, null)?.ToString());
                    }

                    selection.Add(o);
                }
                else {
                    //TODO:这里要根据类型转成相应的,先全转到int

                    var o = new JObject {
                        { "label", string.Format(labelFormat, args) },
                        { "value", Convert.ToInt32(typeof(T).GetProperty(valueKey)?.GetValue(ct, null)?.ToString()) }
                    };
                    if (withParentId && !parentIdKey.IsNullOrEmpty()) {
                        o["parentId"] =
                            Convert.ToInt32(typeof(T).GetProperty(parentIdKey)?.GetValue(ct, null)?.ToString());
                    }

                    selection.Add(o);
                }
            }

            return selection;
        }


        public static JArray GetSelection<T>(this List<T> list, string type, string labelKey, string valueKey,
            IocManager iocManager) {
            var selection = new JArray();
            foreach (var ct in list) {
                var title = "";
                var label = typeof(T).GetProperty(labelKey)?.GetValue(ct, null);

                if (type == "string") {
                    if (label is ILocalizableString localizableString) {
                        using var localizationContext = iocManager.ResolveAsDisposable<ILocalizationContext>();
                        var str = localizableString.Localize(localizationContext.Object);
                        selection.Add(new JObject {
                            { "label", str },
                            { "value", typeof(T).GetProperty(valueKey)?.GetValue(ct, null)?.ToString() }
                        });
                    }

                    else {
                        selection.Add(new JObject {
                            { "label", title },
                            { "value", typeof(T).GetProperty(valueKey)?.GetValue(ct, null)?.ToString() }
                        });
                    }
                }
                else {
                    //TODO:这里要根据类型转成相应的,先全转到int
                    selection.Add(new JObject {
                        { "label", title },
                        { "value", Convert.ToInt32(typeof(T).GetProperty(valueKey)?.GetValue(ct, null)) }
                    });
                }
            }

            return selection;
        }


        public static JArray GetEnumSelection(this Type enumType, bool valueIsNumber = true) {
            var selection = new JArray();
            foreach (var r in Enum.GetValues(enumType)) {
                selection.Add(new JObject {
                    { "label", ((Enum)(object)r).ToDisplayName() },
                    { "value", valueIsNumber ? (int)r : $"{r}" },
                });
            }

            return selection;
        }

        public static string ToDisplayName(this System.Enum value) {
            var attributes =
                (DisplayAttribute[])value.GetType().GetField(value.ToString())!.GetCustomAttributes(
                    typeof(DisplayAttribute), false);
            return attributes.Length > 0 ? attributes[0].Name : value.ToString();
        }
    }
}