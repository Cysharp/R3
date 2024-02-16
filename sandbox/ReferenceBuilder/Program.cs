using MarkdownGenerator;
using System.Reflection;

var f = Factory();
var o = Operator();

// Get absolute path of bin/Debug/TargetFramework/ReferenceBuilder.dll
// Location = /Foo/Bar/R3/sandbox/ReferenceBuilder/bin/Debug/net8.0/ReferenceBuilder.dll
var basePath = Assembly.GetAssembly(typeof(Program))!.Location;
File.WriteAllText(Path.Combine(basePath, "../../../../../../docs/reference_factory.md"), f);
File.WriteAllText(Path.Combine(basePath, "../../../../../../docs/reference_operator.md"), o);

// replace readme
var text = File.ReadAllLines(Path.Combine(basePath, "../../../../../../README.md"));

(int head, int tail)? factoryLines = null;
(int head, int tail)? operatorLines = null;

var searchTail = false;
var i1 = 0;
for (int i = 0; i < text.Length; i++)
{

    const string head = "| Name(Parameter) | ReturnType |";
    if (!searchTail)
    {
        // search head
        if (text[i].Trim() == head)
        {
            i1 = i;
            searchTail = true;
        }
    }
    else
    {
        if (text[i].Trim() == "")
        {
            if (factoryLines == null)
            {
                factoryLines = (i1, i);
            }
            else
            {
                operatorLines = (i1, i);
            }
            searchTail = false;
        }
    }
}

Console.WriteLine(factoryLines!);
Console.WriteLine(operatorLines!);

var newText = new List<string>();
for (int i = 0; i < text.Length; i++)
{
    if (i == factoryLines!.Value.head)
    {
        foreach (var line in f.Split(Environment.NewLine))
        {
            if (line.Trim().Length == 0) continue;
            newText.Add(line);
        }
        i = factoryLines!.Value.tail - 1; // when continue, +1
        while (text[i] == "")
        {
            i++;
        }
        continue;
    }

    if (i == operatorLines!.Value.head)
    {
        foreach (var line in o.Split(Environment.NewLine))
        {
            if (line.Trim().Length == 0) continue;
            newText.Add(line);
        }
        i = operatorLines!.Value.tail - 1;
        while (text[i] == "")
        {
            i++;
        }
        continue;
    }

    newText.Add(text[i]);
}

var nt = string.Join(Environment.NewLine, newText);
File.WriteAllText(Path.Combine(basePath, "../../../../../../README.md"), nt);

static string Factory()
{
    var emptyCommentTable = Enumerable.Empty<string>().ToLookup(x => x, _ => new XmlDocumentComment());
    var t = new MarkdownableType(typeof(R3.Observable), emptyCommentTable);
    return t.ToString();
}

static string Operator()
{
    var emptyCommentTable = Enumerable.Empty<string>().ToLookup(x => x, _ => new XmlDocumentComment());
    var t = new MarkdownableType(typeof(R3.ObservableExtensions), emptyCommentTable);
    return t.ToString();
}

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace MarkdownWikiGenerator
//{
//    class Program
//    {
//        // 0 = dll src path, 1 = dest root
//        static void Main(string[] args)
//        {
//            // put dll & xml on same diretory.
//            var target = "UniRx.dll"; // :)
//            string dest = "md";
//            string namespaceMatch = string.Empty;
//            if (args.Length == 1)
//            {
//                target = args[0];
//            }
//            else if (args.Length == 2)
//            {
//                target = args[0];
//                dest = args[1];
//            }
//            else if (args.Length == 3)
//            {
//                target = args[0];
//                dest = args[1];
//                namespaceMatch = args[2];
//            }

//            var types = MarkdownGenerator.Load(target, namespaceMatch);

//            // Home Markdown Builder
//            var homeBuilder = new MarkdownBuilder();
//            homeBuilder.Header(1, "References");
//            homeBuilder.AppendLine();

//            foreach (var g in types.GroupBy(x => x.Namespace).OrderBy(x => x.Key))
//            {
//                if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);

//                homeBuilder.HeaderWithLink(2, g.Key, g.Key);
//                homeBuilder.AppendLine();

//                var sb = new StringBuilder();
//                foreach (var item in g.OrderBy(x => x.Name))
//                {
//                    homeBuilder.ListLink(MarkdownBuilder.MarkdownCodeQuote(item.BeautifyName), g.Key + "#" + item.BeautifyName.Replace("<", "").Replace(">", "").Replace(",", "").Replace(" ", "-").ToLower());

//                    sb.Append(item.ToString());
//                }

//                File.WriteAllText(Path.Combine(dest, g.Key + ".md"), sb.ToString());
//                homeBuilder.AppendLine();
//            }

//            // Gen Home
//            File.WriteAllText(Path.Combine(dest, "Home.md"), homeBuilder.ToString());
//        }
//    }
//}
