using System;

namespace TtWork.Abp.Core.Extensions {
    public static class TypeExtension {
        public static object ConvertToType(string value, Type targetType) {
            try {
                Type underlyingType = Nullable.GetUnderlyingType(targetType);

                if (underlyingType != null) {
                    if (string.IsNullOrEmpty(value)) {
                        return null;
                    }
                }
                else {
                    underlyingType = targetType;
                }

                return underlyingType.Name switch {
                    "Guid" => Guid.Parse(value),
                    _ => Convert.ChangeType(value, underlyingType)
                };
            }
            catch {
                return null;
            }
        }
    }
}