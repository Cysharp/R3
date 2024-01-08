using MarkdownWikiGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;

public static class Dump
{
    public static void Factory()
    {
        var emptyCommentTable = Enumerable.Empty<string>().ToLookup(x => x, _ => new XmlDocumentComment());
        var t = new MarkdownableType(typeof(R3.Observable), emptyCommentTable);
        Console.WriteLine(t.ToString());
    }

    public static void Operator()
    {
        var emptyCommentTable = Enumerable.Empty<string>().ToLookup(x => x, _ => new XmlDocumentComment());
        var t = new MarkdownableType(typeof(R3.ObservableExtensions), emptyCommentTable);
        Console.WriteLine(t.ToString());
    }
}
