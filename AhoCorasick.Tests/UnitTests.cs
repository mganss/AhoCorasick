using System;
using System.Linq;
using Ganss.Text;
using System.Collections.Generic;
using NUnit.Framework;

namespace Ganss.Text.Tests
{
    public class WordMatchList: List<WordMatch>
    {
        public void Add(int index, string word)
        {
            Add(new WordMatch { Index = index, Word = word });
        }
    }

    public class UnitTests
    {
        [Test]
        public void SearchWikipediaTest()
        {
            // from https://en.wikipedia.org/wiki/Aho%E2%80%93Corasick_string_matching_algorithm
            var ac = new AhoCorasick("a", "ab", "bab", "bc", "bca", "c", "caa");
            var m = ac.Search("abccab").ToList();
            var expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bc" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            CollectionAssert.AreEqual(expected, m);
        }

        [Test]
        public void SimpleTest()
        {
            var ac = new AhoCorasick("a");
            CollectionAssert.AreEqual(new WordMatchList { { 0, "a" } }, ac.Search("a").ToList());
            Assert.AreEqual(0, ac.Search("b").Count());
        }

        [Test]
        public void SearchNullEmptyTest()
        {
            var ac = new AhoCorasick("a");
            var m = ac.Search(null).ToList();
            Assert.AreEqual(0, m.Count);
            m = ac.Search("").ToList();
            Assert.AreEqual(0, m.Count);
        }

        [Test]
        public void SearchMultipleTest()
        {
            var ac = new AhoCorasick("her", "their", "eye", "iris", "he", "is", "si");
            var m = ac.Search("theye iris irisis").ToList();
            var expected = new WordMatchList { { 1, "he" }, { 2, "eye" }, { 6, "iris" }, { 8, "is" }, { 11, "iris" }, { 13, "is" }, { 14, "si" }, { 15, "is" } };
            CollectionAssert.AreEqual(expected, m);
        }

        [Test]
        public void SearchIvankTest()
        {
            // from http://blog.ivank.net/aho-corasick-algorithm-in-as3.html
            var ac = new AhoCorasick("take", "fast", "sofa");
            var m = ac.Search("takeso fasofast fassofatake sosso sofastake so").ToList();
            var expected = new WordMatchList { { 0, "take" }, { 9, "sofa" }, { 11, "fast" }, { 19, "sofa" }, { 23, "take" }, { 34, "sofa" }, { 36, "fast" }, { 39, "take" } };
            CollectionAssert.AreEqual(expected, m);
        }

        [Test]
        public void StringExtensionTest()
        {
            var m = "abc".Contains("abd", "bc", "ab").ToList();
            CollectionAssert.AreEqual(new WordMatchList { { 0, "ab" }, { 1, "bc" } }, m);
        }
    }
}
