# AhoCorasick

[![Version](https://img.shields.io/nuget/v/AhoCorasick.svg)](https://www.nuget.org/packages/AhoCorasick)

This is an implementation of the [Aho-Corasick](https://en.wikipedia.org/wiki/Aho%E2%80%93Corasick_string_matching_algorithm) string matching algorithm. Mostly ported from [xudejian/aho-corasick](https://github.com/xudejian/aho-corasick) in CoffeeScript.

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
