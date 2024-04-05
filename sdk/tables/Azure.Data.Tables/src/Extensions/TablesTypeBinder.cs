// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using Azure.Core;

namespace Azure.Data.Tables
{
    internal class TablesTypeBinder : TypeBinder<IDictionary<string, object>>
    {
        public static TablesTypeBinder Shared { get; } = new();
        protected override void Set<T>(IDictionary<string, object> destination, T value, BoundMemberInfo memberInfo)
        {
            // Remove the ETag and Timestamp properties, as they do not need to be serialized
            if (memberInfo.Name == TableConstants.PropertyNames.ETag || memberInfo.Name == TableConstants.PropertyNames.Timestamp)
            {
                return;
            }

            // Int64 / long / enum should be serialized as string.
            if (value is long or ulong or Enum)
            {
                destination[memberInfo.Name] = value.ToString();
            }
            else
            {
                destination[memberInfo.Name] = value;
            }

            switch (value)
            {
                case byte[]:
                case BinaryData:
                    destination[memberInfo.Name.ToOdataTypeString()] = TableConstants.Odata.EdmBinary;
                    break;
                case long:
                case ulong:
                    destination[memberInfo.Name.ToOdataTypeString()] = TableConstants.Odata.EdmInt64;
                    break;
                case double:
                    destination[memberInfo.Name.ToOdataTypeString()] = TableConstants.Odata.EdmDouble;
                    break;
                case Guid:
                    destination[memberInfo.Name.ToOdataTypeString()] = TableConstants.Odata.EdmGuid;
                    break;
                case DateTimeOffset:
                    destination[memberInfo.Name.ToOdataTypeString()] = TableConstants.Odata.EdmDateTime;
                    break;
                case DateTime:
                    destination[memberInfo.Name.ToOdataTypeString()] = TableConstants.Odata.EdmDateTime;
                    break;
            }

            var newValue = InterceptNullValue(destination[memberInfo.Name], memberInfo);
            if (newValue != null)
            {
                destination[memberInfo.Name] = newValue;
            }
        }

        protected static string InterceptNullValue(object value, BoundMemberInfo memberInfo)
        {
            if (value == null)
            {
                if (memberInfo.Type == typeof(string))
                    return "String.Null";
                if (memberInfo.Type == typeof(DateTime) || memberInfo.Type == typeof(DateTime?))
                    return "DateTime.Null";
                if (memberInfo.Type == typeof(byte[]) || memberInfo.Type == typeof(BinaryData))
                    return "Binary.Null";
                if (memberInfo.Type == typeof(bool) || memberInfo.Type == typeof(bool?))
                    return "Boolean.Null";
                if (memberInfo.Type == typeof(double) || memberInfo.Type == typeof(double?))
                    return "Double.Null";
                if (memberInfo.Type == typeof(Guid) || memberInfo.Type == typeof(Guid?))
                    return "Guid.Null";
                if (memberInfo.Type == typeof(Int32) || memberInfo.Type == typeof(Int32?))
                    return "Int.Null";
                if (memberInfo.Type == typeof(Int64) || memberInfo.Type == typeof(Int64?))
                    return "Long.Null";
            }

            return null;
        }

        protected override bool TryGet<T>(BoundMemberInfo memberInfo, IDictionary<string, object> source, out T value)
        {
            value = default;

            var key = memberInfo.Name switch
            {
                nameof(TableConstants.PropertyNames.ETag) => TableConstants.PropertyNames.EtagOdata,
                _ => memberInfo.Name
            };

            if (!source.TryGetValue(key, out var propertyValue))
            {
                return false;
            }

            if (propertyValue == null)
            {
                value = default(T);
                return true;
            }

            if (typeof(T) == typeof(byte[]))
            {
                value = (T)(object)Convert.FromBase64String(propertyValue as string);
            }
            else if (typeof(T) == typeof(BinaryData))
            {
                value = (T)(object)BinaryData.FromBytes(Convert.FromBase64String(propertyValue as string));
            }
            else if (typeof(T) == typeof(long))
            {
                value = (T)(object)long.Parse(propertyValue as string, CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(long?))
            {
                value = (T)(object)long.Parse(propertyValue as string, CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(ulong))
            {
                value = (T)(object)ulong.Parse(propertyValue as string, CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(ulong?))
            {
                if (propertyValue is string)
                {
                    value = (T)(object)ulong.Parse(propertyValue as string, CultureInfo.InvariantCulture);
                }
                else
                {
                    value = (T)(object)ulong.Parse(propertyValue as string, CultureInfo.InvariantCulture);
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (propertyValue as string == "Double.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)default(double);
                }
                else
                {
                    value = (T)Convert.ChangeType(propertyValue, typeof(double), CultureInfo.InvariantCulture);
                }
            }
            else if (typeof(T) == typeof(double?))
            {
                if (propertyValue as string == "Double.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)null;
                }
                else
                {
                    if (propertyValue is string)
                    {
                        value = (T)(object)double.Parse(propertyValue as string, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        value = (T)(object)(double?)Convert.ChangeType(propertyValue, typeof(double), CultureInfo.InvariantCulture);
                    }
                }
            }
            else if (typeof(T) == typeof(bool))
            {
                if (propertyValue as string == "Boolean.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)false;
                }
                else
                {
                    value = (T)(object)(bool)propertyValue;
                }
            }
            else if (typeof(T) == typeof(bool?))
            {
                if (propertyValue as string == "Boolean.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)null;
                }
                else
                {
                    if (propertyValue is string)
                    {
                        value = (T)(object)bool.Parse(propertyValue as string);
                    }
                    else
                    {
                        value = (T)(object)(bool?)propertyValue;
                    }
                }
            }
            else if (typeof(T) == typeof(Guid))
            {
                if (propertyValue as string == "Guid.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)Guid.Empty;
                }
                else
                {
                    value = (T)(object)Guid.Parse(propertyValue as string);
                }
            }
            else if (typeof(T) == typeof(Guid?))
            {
                if (propertyValue as string == "Guid.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)null;
                }
                else
                {
                    value = (T)(object)Guid.Parse(propertyValue as string);
                }
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                value = (T)(object)DateTimeOffset.Parse(propertyValue as string, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else if (typeof(T) == typeof(DateTimeOffset?))
            {
                value = (T)(object)DateTimeOffset.Parse(propertyValue as string, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                if (propertyValue as string == "DateTime.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)default(DateTime);
                }
                else
                {
                    value = (T)(object)DateTime.Parse((string)propertyValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
            }
            else if (typeof(T) == typeof(DateTime?))
            {
                if (propertyValue as string == "DateTime.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)null;
                }
                else
                {
                    value = (T)(object)DateTime.Parse(propertyValue as string, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
            }
            else if (typeof(T) == typeof(string))
            {
                if (propertyValue as string == "String.Null")
                {
                    value = (T)(object)null;
                }
                else
                {
                    value = (T)(object)(propertyValue as string);
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (propertyValue as string == "Int.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)default(int);
                }
                else
                {
                    value = (T)(object)(int)propertyValue;
                }
            }
            else if (typeof(T) == typeof(int?))
            {
                if (propertyValue as string == "Int.Null" || propertyValue as string == "String.Null")
                {
                    value = (T)(object)null;
                }
                else
                {
                    if (propertyValue is string)
                    {
                        value = (T)(object)int.Parse(propertyValue as string, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        value = (T)(object)(int?)propertyValue;
                    }
                }
            }
            else if (typeof(T).IsEnum)
            {
                value = (T)Enum.Parse(memberInfo.Type, propertyValue as string);
            }
            else if (typeof(T).IsGenericType &&
                     typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) &&
                     typeof(T).GetGenericArguments() is { Length: 1 } arguments &&
                     arguments[0].IsEnum)
            {
                value = (T)Enum.Parse(arguments[0], propertyValue as string);
            }
            else if (typeof(T) == typeof(ETag))
            {
                value = (T)(object)new ETag(propertyValue as string);
            }
            return true;
        }
    }
}
