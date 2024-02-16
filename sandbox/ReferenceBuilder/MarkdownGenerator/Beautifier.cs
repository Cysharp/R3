#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkdownGenerator;

public static class Beautifier
{
    public static string BeautifyType(Type t, bool isFull = false)
    {
        if (t == null) return "";
        if (t == typeof(void)) return "void";
        if (t.IsArray)
        {
            var innerFormat = BeautifyType(t.GetElementType(), isFull);
            return innerFormat + "[]";
        }

        if (!t.IsGenericType) return (isFull) ? t.FullName : t.Name;

        {
            var innerFormat = string.Join(", ", t.GetGenericArguments().Select(x => BeautifyType(x)));
            return Regex.Replace(isFull ? t.GetGenericTypeDefinition().FullName : t.GetGenericTypeDefinition().Name, @"`.+$", "") + "<" + innerFormat + ">";
        }
    }

    public static string ToMarkdownMethodInfo(MethodInfo methodInfo)
    {
        var isExtension = IsExtensionMethod(methodInfo);

        var seq = methodInfo.GetParameters().Select(x =>
        {
            var isParams = IsParamsParameter(x);
            var refKind = IsOutParameter(x) ? "out " : IsRefParameter(x) ? "ref " : IsInParameter(x) ? "in " : "";
            var prefix = isParams ? "params " : refKind;

            var defaultValue = "default";
            if (x.DefaultValue != null)
            {
                if (x.DefaultValue.GetType().IsEnum)
                {
                    defaultValue = x.DefaultValue.GetType().Name + "." + x.DefaultValue.ToString();
                }
                else
                {
                    defaultValue = x.DefaultValue.ToString().ToLower();
                }
            }
            var suffix = x.HasDefaultValue ? (" = " + defaultValue) : "";
            return prefix + "`" + BeautifyType(x.ParameterType) + "` " + x.Name + suffix;
        });

        // NOTE: modify **
        return "**" + methodInfo.Name + "**" + "(" + (isExtension ? "this " : "") + string.Join(", ", seq) + ")";
    }

    static bool IsExtensionMethod(MethodInfo method)
    {
        return method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
    }

    static bool IsParamsParameter(ParameterInfo parameter)
    {
        return parameter.IsDefined(typeof(ParamArrayAttribute), false);
    }

    static bool IsOutParameter(ParameterInfo parameter)
    {
        return parameter.IsOut;
    }

    static bool IsRefParameter(ParameterInfo parameter)
    {
        return parameter.ParameterType.IsByRef && !parameter.IsOut;
    }

    static bool IsInParameter(ParameterInfo parameter)
    {
        return parameter.IsIn && parameter.ParameterType.IsByRef;
    }
}
