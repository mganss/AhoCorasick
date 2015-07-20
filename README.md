# AhoCorasick

[![Version](https://img.shields.io/nuget/v/AhoCorasick.svg)](https://www.nuget.org/packages/AhoCorasick)

This is an implementation of the [Aho-Corasick](https://en.wikipedia.org/wiki/Aho%E2%80%93Corasick_string_matching_algorithm) string matching algorithm for .NET (PCL) and SQL Server (SQL CLR). Mostly ported from [xudejian/aho-corasick](https://github.com/xudejian/aho-corasick) in CoffeeScript.

## Usage

```C#
var ac = new AhoCorasick("a", "ab", "bab", "bc", "bca", "c", "caa");
var results = ac.Search("abccab").ToList();

Assert.AreEqual(0, results[0].Index); // index into the searched text
Assert.AreEqual("a", results[0].Word); // matched word
// ...
```

or

```C#
var results = "abccab".Contains("a", "ab", "bab", "bc", "bca", "c", "caa").ToList();
```

### Custom char comparison

You can optionally supply an `IEqualityComparer<char>` to perform custom char comparisons when searching for substrings. Several implementations with comparers that mirror `StringComparer` are included.

```C#
var results = "AbCcab".Contains(CharComparer.OrdinalIgnoreCase, "a", "ab", "c").ToList();
```

## SQL CLR Functions

There are also several SQL CLR user defined functions that can be used to perform fast substring matching
in Microsoft SQL Server. To use this:

1. Make sure you have [enabled CLR integration](https://msdn.microsoft.com/en-us/library/ms131048.aspx)
2. Execute [AhoCorasick.SqlClr_Create.sql](AhoCorasick.SqlClr/dist/AhoCorasick.SqlClr_Create.sql)

For one-off queries, you can use the functions that rebuild the trie on each query, e.g.

```SQL
select top(100) * from Posts P
where dbo.ContainsWords((select Word from Words for xml raw, root('root')), P.Body, 'o') = 1
```

The words to match are always supplied as XML where the values are taken from the first attribute of all elements directly beneath the root node. Be careful to select the word column as the only or first column otherwise you'll end up matching the wrong words. The XML in the example above looks like this:

```XML
<root>
  <row Word="Aachen" />
  <row Word="Aaliyah" />
  <row Word="aardvark" />
  ...
</root>
```

[Here's more](https://www.simple-talk.com/sql/learn-sql-server/using-the-for-xml-clause-to-return-query-results-as-xml/) about FOR XML.

The last parameter in the function indicates the culture to use since there is no way to use SQL Server collations in SQL CLR code. Values can be:

|Value|Character comparison|
|-----|--------------------|
|c|Current Culture|
|n|Invariant Culture|
|o or Empty|Ordinal|
|Culture name, e.g. "de-de"|Specific [.NET Culture](https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.name.aspx)|

The culture identifier can be suffixed by `:i` indicating case-insensitive matching.

### Static objects

The function in the example above has the problem that the trie is rebuilt for each query even though the input always stays the same. To overcome this problem, there a number of functions to manage the creation and destruction of static objects whose handles can be saved in SQL variables. Example:

```SQL
declare @ac nvarchar(32);
set @ac = dbo.CreateAhoCorasick((select Word from Words for xml raw, root('root')), 'en-us:i');
select * from Posts P
where dbo.ContainsWordsByObject(P.Body, @ac) = 1;
```

This is a lot faster than the first example because the trie is created only once and then reused for each row in the query. The handle (@ac) is a hash value generated from the words to match and the culture. The corresponding object is saved in a static dictionary. You can list the currently active objects using `dbo.ListAhoCorasick()`, remove all objects using `dbo.ClearAhoCorasick()` or remove only one object using `dbo.DeleteAhoCorasick(@ac)`.

### Getting all matches

The examples above only checked if the words occurred in the queried texts. If you want to get the matched words and the indexes where they occur in the queried texts you can use the supplied table-valued functions. For example:

```SQL
declare @ac nvarchar(32);
set @ac = dbo.CreateAhoCorasick((select Word from Words for xml raw, root('root')), 'o');
select top(100) * from Posts P
cross apply dbo.ContainsWordsTableByObject(P.Body, @ac) W
```

This will return a table such as this:

|ID   |Body   |Index   |Word   |
|---|---|---|---|
|1 |What factors related...|5|factor|
|1 |What factors related...|6|actor|
|1 |What factors related...|5|factors|
|...|

### Word boundaries

There are also functions that return only matches occuring at word boundaries: `dbo.ContainsWordsBoundedByObject()` and `dbo.ContainsWordsBoundedTableByObject()`. Word boundaries here are the same as [`\b` in regexes](http://www.regular-expressions.info/wordboundaries.html), i.e. matches will occur as if words were specified as `\bword\b`.

### Forcing parallelism

Although these kinds of queries lend themselves very well to parallel execution, SQL Server tends to overestimate the cost of parallel queries and builds non-parallel plans most of the time where user defined functions are involved. You can force a parallel plan by using a trace flag (more about this [here](http://www.regular-expressions.info/wordboundaries.html)):

```SQL
declare @ac nvarchar(32);
set @ac = dbo.CreateAhoCorasick((select Word from Words for xml raw, root('root')), 'en-us:i');
select * from Posts P
where dbo.ContainsWordsBoundedByObject(P.Body, @ac) = 1
OPTION (RECOMPILE, QUERYTRACEON 8649)
```

Parallel operators are identified by a yellow badge with two arrows in the query plan.

### Performance

Here's a benchmark searching for ~5000 words (average length 7) in ~250,000 texts (average length ~900):

|SQL|AhoCorasick|
|---|-----------|
|560s|7s|

The SQL query used was this:

```SQL
select * from Posts P
where exists (select * from Words W where CHARINDEX(W.Word, P.Text) > 0)
```

#### But I can simply use full-text search

No. The [CONTAINS](https://msdn.microsoft.com/en-us/library/ms187787.aspx) predicate can only search for a single literal or variable at a time. You can't use it in a join or subquery to search for a column value of a table in the query, i.e. this won't work:

```SQL
select * from Posts P
where exists (select * from Words W where CONTAINS(P.Text, W.Word))
```

If you know of a way to make this work using FTS (perhaps using a cursor?) let me know.
