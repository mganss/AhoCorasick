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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ac.Search("a").ToList(), Is.EquivalentTo(new WordMatchList { { 0, "a" } }));
                Assert.That(ac.Search("b"), Is.Empty);
            }
        }

        [Test]
        public void SearchNullEmptyTest()
        {
            var ac = new AhoCorasick("a");
            var m = ac.Search(null).ToList();
            Assert.That(m, Is.Empty);
            m = [.. ac.Search("")];
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
            m = [.."abc".Contains(new List<string> { "abd", "bc", "ab" })];
            Assert.That(m, Is.EquivalentTo(new WordMatchList { { 0, "ab" }, { 1, "bc" } }));
            m = [.."ABC".Contains(CharComparer.OrdinalIgnoreCase, "abd", "bc", "ab")];
            Assert.That(m, Is.EquivalentTo(new WordMatchList { { 0, "ab" }, { 1, "bc" } }));
            m = [.."ABC".Contains(CharComparer.OrdinalIgnoreCase, new List<string> { "abd", "bc", "ab" })];
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ac.Search("a").ToList(), Is.EquivalentTo(new WordMatchList { { 0, "a" } }));
                Assert.That(ac.Search("b"), Is.Empty);
            }

            ac = new AhoCorasick(CharComparer.OrdinalIgnoreCase, new List<string> { "a", "ab", "bab", "bC", "bca", "c", "caa" });
            var m = ac.Search("abCcab").ToList();
            var expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bC" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));

            ac = new AhoCorasick();
            ac.Add("a");
            ac.BuildFail();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ac.Search("a").ToList(), Is.EquivalentTo(new WordMatchList { { 0, "a" } }));
                Assert.That(ac.Search("b"), Is.Empty);
            }

            ac = new AhoCorasick(CharComparer.Create(CultureInfo.InvariantCulture, true), "a", "ab", "bab", "bc", "bca", "c", "caa");
            m = [.. ac.Search("abccab")];
            expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bc" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void AddAfterBuildFailTest()
        {
            var ac = new AhoCorasick("aca");
            ac.Add(new List<string> { "ca" });
            var m = ac.Search("aca").ToList();
            var expected = new WordMatchList { { 0, "aca" }, { 1, "ca" } };
            Assert.That(m, Is.EquivalentTo(expected));

            ac = new AhoCorasick("a", "aca");
            ac.Add("ca");
            ac.BuildFail();
            m = [.. ac.Search("aca")];
            expected = new WordMatchList { { 0, "a" }, { 0, "aca" }, { 1, "ca" }, { 2, "a" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void SearchBuildsFailNodesTest()
        {
            // Add(string) does not build the failure nodes, Search has to do it.
            var ac = new AhoCorasick("aca");
            ac.Add("ca");
            var expected = new WordMatchList { { 0, "aca" }, { 1, "ca" } };
            Assert.That(ac.Search("aca").ToList(), Is.EquivalentTo(expected));

            // A second search gives the same result.
            Assert.That(ac.Search("aca").ToList(), Is.EquivalentTo(expected));

            // The same for an instance that was never built at all.
            ac = new AhoCorasick();
            ac.Add("ca");
            Assert.That(ac.Search("aca").ToList(), Is.EquivalentTo(new WordMatchList { { 1, "ca" } }));
        }

        [Test]
        public void LongWordTest()
        {
            // Trie.Add walks the word rather than recursing, so the stack does not limit how long a
            // word can be, and a node builds its prefix on demand rather than holding a copy, so the
            // memory needed is linear in the length of the word. Holding a copy in every node would
            // need gigabytes for the word below.
            const int length = 50000;
            var longWord = new string('a', length) + "b";
            var ac = new AhoCorasick(longWord, "ab");
            var m = ac.Search(longWord).ToList();
            var expected = new WordMatchList { { 0, longWord }, { length - 1, "ab" } };
            Assert.That(m, Is.EquivalentTo(expected));
        }

        [Test]
        public void SearchOrderTest()
        {
            // Matches come out by the position they end at, longest first within a position. Not part
            // of the contract, but it is what callers get, so changes to Search should not alter it.
            var ac = new AhoCorasick("a", "ab", "bab", "bc", "bca", "c", "caa");
            var m = ac.Search("abccab").ToList();
            var expected = new WordMatchList { { 0, "a" }, { 0, "ab" }, { 1, "bc" }, { 2, "c" }, { 3, "c" }, { 4, "a" }, { 4, "ab" } };
            Assert.That(m, Is.EqualTo(expected));
        }

        [Test]
        public void NeedsBuildFailTest()
        {
            var ac = new AhoCorasick();
            Assert.That(ac.NeedsBuildFail, Is.False, "nothing has been added yet");

            ac.Add("aca");
            Assert.That(ac.NeedsBuildFail, "Add(string) does not build the failure nodes");

            ac.Search("aca").ToList();
            Assert.That(ac.NeedsBuildFail, Is.False, "Search builds the failure nodes");

            ac.Add("ca");
            Assert.That(ac.NeedsBuildFail);

            ac.BuildFail();
            Assert.That(ac.NeedsBuildFail, Is.False);

            // Add(IEnumerable<string>) builds them itself.
            ac.Add(new List<string> { "c" });
            Assert.That(ac.NeedsBuildFail, Is.False);

            // The words of the constructor are added through Add(IEnumerable<string>).
            Assert.That(new AhoCorasick("a").NeedsBuildFail, Is.False);
        }

        [Test]
        public void BuildFailIgnoresNodeTest()
        {
            // The parameter cannot be honoured, a node can fail to a node in an unrelated branch.
            var ac = new SubtreeAhoCorasick();
            ac.Add("aca");
            ac.Add("ca");
            ac.BuildFailFromFirstChild();

            Assert.That(ac.NeedsBuildFail, Is.False);
            var expected = new WordMatchList { { 0, "aca" }, { 1, "ca" } };
            Assert.That(ac.Search("aca").ToList(), Is.EquivalentTo(expected));
        }

        private sealed class SubtreeAhoCorasick : AhoCorasick
        {
            public void BuildFailFromFirstChild()
            {
                foreach (var child in Trie.Next.Values)
                {
                    BuildFail(child);
                    return;
                }
            }
        }
    }
}
