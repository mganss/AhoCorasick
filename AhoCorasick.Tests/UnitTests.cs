using System;
using System.Linq;
using Ganss.Text;
using System.Collections.Generic;
using NUnit.Framework;
using System.Globalization;

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
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void SimpleTest()
        {
            var ac = new AhoCorasick("a");
            Assert.That(ac.Search("a").ToList(), Is.EquivalentTo(new WordMatchList { { 0, "a" } }));
            Assert.That(ac.Search("b"), Is.Empty);
        }

        [Test]
        public void SearchNullEmptyTest()
        {
            var ac = new AhoCorasick("a");
            var m = ac.Search(null).ToList();
            Assert.That(m, Is.Empty);
            m = ac.Search("").ToList();
            Assert.That(m, Is.Empty);
        }

        [Test]
        public void SearchMultipleTest()
        {
            var ac = new AhoCorasick("her", "their", "eye", "iris", "he", "is", "si");
            var m = ac.Search("theye iris irisis").ToList();
            var expected = new WordMatchList { { 1, "he" }, { 2, "eye" }, { 6, "iris" }, { 8, "is" }, { 11, "iris" }, { 13, "is" }, { 14, "si" }, { 15, "is" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void SearchIvankTest()
        {
            // from http://blog.ivank.net/aho-corasick-algorithm-in-as3.html
            var ac = new AhoCorasick("take", "fast", "sofa");
            var m = ac.Search("takeso fasofast fassofatake sosso sofastake so").ToList();
            var expected = new WordMatchList { { 0, "take" }, { 9, "sofa" }, { 11, "fast" }, { 19, "sofa" }, { 23, "take" }, { 34, "sofa" }, { 36, "fast" }, { 39, "take" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void StringExtensionTest()
        {
            var m = "abc".Contains("abd", "bc", "ab").ToList();
            Assert.That(m, Is.EquivalentTo(new WordMatchList { { 0, "ab" }, { 1, "bc" } }));
            m = "abc".Contains(new List<string> { "abd", "bc", "ab" }).ToList();
            Assert.That(m, Is.EquivalentTo(new WordMatchList { { 0, "ab" }, { 1, "bc" } }));
            m = "ABC".Contains(CharComparer.OrdinalIgnoreCase, "abd", "bc", "ab").ToList();
            Assert.That(m, Is.EquivalentTo(new WordMatchList { { 0, "ab" }, { 1, "bc" } }));
            m = "ABC".Contains(CharComparer.OrdinalIgnoreCase, new List<string> { "abd", "bc", "ab" }).ToList();
            Assert.That(m, Is.EquivalentTo(new WordMatchList { { 0, "ab" }, { 1, "bc" } }));
        }

        [Test]
        public void UpperCaseTest()
        {
            var ac = new AhoCorasick("a", "ab", "bab", "bC", "bca", "c", "caa");
            var m = ac.Search("abCcab").ToList();
            var expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bC" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void OrdinalIgnoreCaseTest()
        {
            var ac = new AhoCorasick(CharComparer.OrdinalIgnoreCase, "a", "ab", "bab", "bC", "bca", "c", "caa");
            var m = ac.Search("abCcab").ToList();
            var expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bC" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void OverloadsTest()
        {
            var ac = new AhoCorasick(new List<string> { "a" });
            Assert.That(ac.Search("a").ToList(), Is.EquivalentTo(new WordMatchList { { 0, "a" } }));
            Assert.That(ac.Search("b"), Is.Empty);

            ac = new AhoCorasick(CharComparer.OrdinalIgnoreCase, new List<string> { "a", "ab", "bab", "bC", "bca", "c", "caa" });
            var m = ac.Search("abCcab").ToList();
            var expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bC" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));

            ac = new AhoCorasick();
            ac.Add("a");
            ac.BuildFail();
            Assert.That(ac.Search("a").ToList(), Is.EquivalentTo(new WordMatchList { { 0, "a" } }));
            Assert.That(ac.Search("b"), Is.Empty);

            ac = new AhoCorasick(CharComparer.Create(CultureInfo.InvariantCulture, true), "a", "ab", "bab", "bc", "bca", "c", "caa");
            m = ac.Search("abccab").ToList();
            expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bc" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }
    }
}
