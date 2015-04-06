using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Xml.Linq;
using System.Linq;
using Ganss.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

public partial class UserDefinedFunctions
{
    private static AhoCorasick BuildAhoCorasick(SqlXml xml, SqlString culture)
    {
        var xe = XElement.Load(xml.CreateReader());
        var words = xe.Elements().Select(e => e.FirstAttribute.Value);
        var c = culture.Value.Split(':');
        var ignoreCase = c.Length > 1 && c[1] == "i";
        CharComparer cc;
        switch (c[0])
        {
            case "c":
                cc = CharComparer.Create(CultureInfo.CurrentCulture, ignoreCase);
                break;
            case "n":
                cc = CharComparer.Create(CultureInfo.InvariantCulture, ignoreCase);
                break;
            case "o":
            case "":
                cc = ignoreCase ? CharComparer.OrdinalIgnoreCase : CharComparer.Ordinal;
                break;
            default:
                cc = CharComparer.Create(CultureInfo.GetCultureInfo(c[0]), ignoreCase);
                break;
        }
        var ac = new AhoCorasick(cc, words);
        return ac;
    }

    [SqlFunction(FillRowMethodName = "FillRow", TableDefinition = @"""Index"" int, Word nvarchar(MAX)",
        IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static IEnumerable ContainsWordsTable(SqlXml xml, SqlString text, SqlString culture)
    {
        var ac = BuildAhoCorasick(xml, culture);
        var matches = ac.Search(text.Value);

        return matches;
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static bool ContainsWords(SqlXml xml, SqlString text, SqlString culture)
    {
        return ContainsWordsTable(xml, text, culture).Cast<WordMatch>().Any();
    }

    public static void FillRow(object obj, out int index, out SqlString word)
    {
        var match = (WordMatch)obj;
        index = match.Index;
        word = match.Word;
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static string CreateAhoCorasick(SqlXml xml, SqlString culture)
    {
        var ac = BuildAhoCorasick(xml, culture);
        var hash = Hash(xml.Value + culture.Value);
        Objects[hash] = ac;
        return hash;
    }

    [SqlFunction(IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static bool DeleteAhoCorasick(SqlString obj)
    {
        Objects.Remove(obj.Value);
        return true;
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static bool ClearAhoCorasick()
    {
        Objects.Clear();
        return true;
    }

    [SqlFunction(FillRowMethodName = "FillRow", TableDefinition = @"""Index"" int, Word nvarchar(MAX)", IsDeterministic = true, IsPrecise = true,
        DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static IEnumerable ContainsWordsTableByObject(SqlString text, SqlString obj)
    {
        var ac = Objects[obj.Value];
        var matches = ac.Search(text.Value);

        return matches;
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static bool ContainsWordsByObject(SqlString text, SqlString obj)
    {
        return ContainsWordsTableByObject(text, obj).Cast<WordMatch>().Any();
    }

    [SqlFunction(FillRowMethodName = "FillRow", TableDefinition = @"""Index"" int, Word nvarchar(MAX)", IsDeterministic = true, IsPrecise = true,
        DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static IEnumerable ContainsWordsBoundedTableByObject(SqlString text, SqlString obj)
    {
        var ac = Objects[obj.Value];
        var t = text.Value;
        var matches = ac.Search(t).Cast<WordMatch>().Where(m =>
        {
            var start = m.Index == 0 || !char.IsLetterOrDigit(t[m.Index - 1]);
            var end = (m.Index + m.Word.Length) == t.Length || !char.IsLetterOrDigit(t[m.Index + m.Word.Length]);
            return start && end;
        });

        return matches;
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true, DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static bool ContainsWordsBoundedByObject(SqlString text, SqlString obj)
    {
        return ContainsWordsBoundedTableByObject(text, obj).Cast<WordMatch>().Any();
    }

    [SqlFunction(FillRowMethodName = "FillRowList", TableDefinition = @"Hash nvarchar(MAX)", IsDeterministic = true, IsPrecise = true,
        DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None)]
    public static IEnumerable ListAhoCorasick()
    {
        return Objects.Keys;
    }

    public static void FillRowList(object obj, out SqlString word)
    {
        word = (string)obj;
    }

    private static string Hash(string s)
    {
        return string.Concat(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(s)).Select(b => b.ToString("X2")));
    }

    private static readonly Dictionary<string, AhoCorasick> Objects = new Dictionary<string, AhoCorasick>();
}
