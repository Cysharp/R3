#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MarkdownGenerator;

public class MarkdownableType
{
    readonly Type type;
    readonly ILookup<string, XmlDocumentComment> commentLookup;

    public string Namespace => type.Namespace;
    public string Name => type.Name;
    public string BeautifyName => Beautifier.BeautifyType(type);

    public MarkdownableType(Type type, ILookup<string, XmlDocumentComment> commentLookup)
    {
        this.type = type;
        this.commentLookup = commentLookup;
    }

    MethodInfo[] GetMethods()
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any() && !x.IsPrivate)
            .ToArray();
    }

    PropertyInfo[] GetProperties()
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
            .Where(y =>
            {
                var get = y.GetGetMethod(true);
                var set = y.GetSetMethod(true);
                if (get != null && set != null)
                {
                    return !(get.IsPrivate && set.IsPrivate);
                }
                else if (get != null)
                {
                    return !get.IsPrivate;
                }
                else if (set != null)
                {
                    return !set.IsPrivate;
                }
                else
                {
                    return false;
                }
            })
            .ToArray();
    }

    FieldInfo[] GetFields()
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.SetField)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any() && !x.IsPrivate)
            .ToArray();
    }

    EventInfo[] GetEvents()
    {
        return type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
            .ToArray();
    }

    FieldInfo[] GetStaticFields()
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.SetField)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any() && !x.IsPrivate)
            .ToArray();
    }

    PropertyInfo[] GetStaticProperties()
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
            .Where(y =>
            {
                var get = y.GetGetMethod(true);
                var set = y.GetSetMethod(true);
                if (get != null && set != null)
                {
                    return !(get.IsPrivate && set.IsPrivate);
                }
                else if (get != null)
                {
                    return !get.IsPrivate;
                }
                else if (set != null)
                {
                    return !set.IsPrivate;
                }
                else
                {
                    return false;
                }
            })
            .ToArray();
    }

    MethodInfo[] GetStaticMethods()
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any() && !x.IsPrivate)
            .ToArray();
    }

    EventInfo[] GetStaticEvents()
    {
        return type.GetEvents(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
            .ToArray();
    }
    void BuildTable<T>(MarkdownBuilder mb, string label, T[] array, IEnumerable<XmlDocumentComment> docs, Func<T, string> type, Func<T, string> name, Func<T, string> finalName)
    {
        if (array.Any())
        {
            if (!string.IsNullOrEmpty(label))
            {
                mb.AppendLine(label);
                mb.AppendLine();
            }


            // NOTE: configure, no summary, return is right
            //string[] head = (this.type.IsEnum)
            //    ? new[] { "Value", "Name", "Summary" }
            //    : new[] { "Type", "Name", "Summary" };

            string[] head = (this.type.IsEnum)
                ? new[] { "Name", "Value" }
                : new[] { "Name(Parameter)", "ReturnType" };

            IEnumerable<T> seq = array;
            if (!this.type.IsEnum)
            {
                seq = array.OrderBy(x => name(x));
            }

            var data = seq.Select(item2 =>
            {
                var summary = docs.FirstOrDefault(x => x.MemberName == name(item2) || x.MemberName.StartsWith(name(item2) + "`"))?.Summary ?? "";
                // NOTE: modify
                // return new[] { MarkdownBuilder.MarkdownCodeQuote(type(item2)), finalName(item2), summary };
                return new[] { finalName(item2), MarkdownBuilder.MarkdownCodeQuote(type(item2)) };
            });

            mb.Table(head, data);
            mb.AppendLine();
        }
    }

    public override string ToString()
    {
        var mb = new MarkdownBuilder();

        //mb.HeaderWithCode(2, Beautifier.BeautifyType(type, false));
        //mb.AppendLine();

        //var desc = commentLookup[type.FullName].FirstOrDefault(x => x.MemberType == MemberType.Type)?.Summary ?? "";
        //if (desc != "")
        //{
        //    mb.AppendLine(desc);
        //}
        //{
        //    var sb = new StringBuilder();

        //    var stat = (type.IsAbstract && type.IsSealed) ? "static " : "";
        //    var abst = (type.IsAbstract && !type.IsInterface && !type.IsSealed) ? "abstract " : "";
        //    var classOrStructOrEnumOrInterface = type.IsInterface ? "interface" : type.IsEnum ? "enum" : type.IsValueType ? "struct" : "class";

        //    sb.AppendLine($"public {stat}{abst}{classOrStructOrEnumOrInterface} {Beautifier.BeautifyType(type, true)}");
        //    var impl = string.Join(", ", new[] { type.BaseType }.Concat(type.GetInterfaces()).Where(x => x != null && x != typeof(object) && x != typeof(ValueType)).Select(x => Beautifier.BeautifyType(x)));
        //    if (impl != "")
        //    {
        //        sb.AppendLine("    : " + impl);
        //    }

        //    mb.Code("csharp", sb.ToString());
        //}

        //mb.AppendLine();

        if (type.IsEnum)
        {
            var underlyingEnumType = Enum.GetUnderlyingType(type);

            var enums = Enum.GetNames(type)
                .Select(x => new { Name = x, Value = (Convert.ChangeType(Enum.Parse(type, x), underlyingEnumType)) })
                .OrderBy(x => x.Value)
                .ToArray();

            BuildTable(mb, "Enum", enums, commentLookup[type.FullName], x => x.Value.ToString(), x => x.Name, x => x.Name);
        }
        else
        {
            BuildTable(mb, "Fields", GetFields(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.FieldType), x => x.Name, x => x.Name);
            BuildTable(mb, "Properties", GetProperties(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.PropertyType), x => x.Name, x => x.Name);
            BuildTable(mb, "Events", GetEvents(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.EventHandlerType), x => x.Name, x => x.Name);
            BuildTable(mb, "Methods", GetMethods(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.ReturnType), x => x.Name, x => Beautifier.ToMarkdownMethodInfo(x));
            BuildTable(mb, "Static Fields", GetStaticFields(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.FieldType), x => x.Name, x => x.Name);
            // BuildTable(mb, "Static Properties", GetStaticProperties(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.PropertyType), x => x.Name, x => x.Name);
            // BuildTable(mb, "Static Methods", GetStaticMethods(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.ReturnType), x => x.Name, x => Beautifier.ToMarkdownMethodInfo(x));

            BuildTable(mb, "", GetStaticMethods(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.ReturnType), x => x.Name, x => Beautifier.ToMarkdownMethodInfo(x));
            BuildTable(mb, "Static Events", GetStaticEvents(), commentLookup[type.FullName], x => Beautifier.BeautifyType(x.EventHandlerType), x => x.Name, x => x.Name);
        }

        return mb.ToString();
    }
}


public static class MarkdownGenerator
{
    public static MarkdownableType[] Load(string dllPath, string namespaceMatch)
    {
        var xmlPath = Path.Combine(Directory.GetParent(dllPath).FullName, Path.GetFileNameWithoutExtension(dllPath) + ".xml");

        XmlDocumentComment[] comments = new XmlDocumentComment[0];
        if (File.Exists(xmlPath))
        {
            comments = VSDocParser.ParseXmlComment(XDocument.Parse(File.ReadAllText(xmlPath)), namespaceMatch);
        }
        var commentsLookup = comments.ToLookup(x => x.ClassName);

        var namespaceRegex =
            !string.IsNullOrEmpty(namespaceMatch) ? new Regex(namespaceMatch) : null;

        var markdownableTypes = new[] { Assembly.LoadFrom(dllPath) }
            .SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null);
                }
                catch
                {
                    return Type.EmptyTypes;
                }
            })
            .Where(x => x != null)
            .Where(x => x.IsPublic && !typeof(Delegate).IsAssignableFrom(x) && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
            .Where(x => IsRequiredNamespace(x, namespaceRegex))
            .Select(x => new MarkdownableType(x, commentsLookup))
            .ToArray();


        return markdownableTypes;
    }

    static bool IsRequiredNamespace(Type type, Regex regex)
    {
        if (regex == null)
        {
            return true;
        }
        return regex.IsMatch(type.Namespace != null ? type.Namespace : string.Empty);
    }
}
