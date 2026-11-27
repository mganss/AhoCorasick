using System;
using System.Collections.Generic;
using System.Linq;
using Ganss.Text;
using NUnit.Framework;

namespace Ganss.Text.Tests
{
    /// <summary>
    /// Randomized differential tests: compares <see cref="AhoCorasick.Search"/> against a brute force
    /// search over the same words. The seeds are fixed so that a failure is reproducible, and a failing
    /// case is shrunk to a minimal one before it is reported.
    /// </summary>
    public class RandomizedTests
    {
        // A single batch of words, and how they are handed to AhoCorasick:
        // via Add(IEnumerable<string>), which builds the failure links itself, or
        // via Add(string) per word followed by an explicit BuildFail().
        private sealed class Batch
        {
            public List<string> Words = [];
            public bool ViaEnumerable;

            public Batch Clone() => new() { Words = [.. Words], ViaEnumerable = ViaEnumerable };
        }

        private const string Alphabet = "abc";
        private const int CasesPerSeed = 2500;

        private static string RandomString(Random random, int maxLength)
        {
            var chars = new char[random.Next(1, maxLength + 1)];

            for (int i = 0; i < chars.Length; i++)
                chars[i] = Alphabet[random.Next(Alphabet.Length)];

            return new string(chars);
        }

        private static List<string> RandomWords(Random random, int maxCount, int maxLength) =>
            [.. Enumerable.Range(0, random.Next(1, maxCount + 1)).Select(_ => RandomString(random, maxLength))];

        /// <summary>
        /// Searches <paramref name="text"/> for the words of all batches without using the trie.
        /// </summary>
        private static List<WordMatch> BruteForceSearch(List<Batch> batches, string text)
        {
            var matches = new List<WordMatch>();

            // A word added more than once is still only one entry in the trie, hence Distinct.
            foreach (var word in batches.SelectMany(b => b.Words).Distinct())
                for (int i = 0; i + word.Length <= text.Length; i++)
                    if (string.CompareOrdinal(text, i, word, 0, word.Length) == 0)
                        matches.Add(new WordMatch { Index = i, Word = word });

            return matches;
        }

        private static List<WordMatch> TrieSearch(List<Batch> batches, string text)
        {
            var ac = new AhoCorasick();

            foreach (var batch in batches)
            {
                if (batch.ViaEnumerable)
                {
                    ac.Add(batch.Words);
                }
                else
                {
                    foreach (var word in batch.Words) ac.Add(word);
                    ac.BuildFail();
                }
            }

            return [.. ac.Search(text)];
        }

        // Search is free to report matches in any order, so compare them as sorted sets of index/word pairs.
        private static List<string> Normalize(IEnumerable<WordMatch> matches) =>
            [.. matches.Select(m => $"{m.Index}:{m.Word}").OrderBy(s => s, StringComparer.Ordinal)];

        private static bool Mismatches(List<Batch> batches, string text) =>
            !Normalize(TrieSearch(batches, text)).SequenceEqual(Normalize(BruteForceSearch(batches, text)));

        /// <summary>
        /// Enumerates cases that are smaller than the given one: one word dropped, or one character of
        /// the text dropped.
        /// </summary>
        private static IEnumerable<(List<Batch> Batches, string Text)> Smaller(List<Batch> batches, string text)
        {
            for (int i = 0; i < batches.Count; i++)
            {
                for (int j = 0; j < batches[i].Words.Count; j++)
                {
                    var candidate = batches.Select(b => b.Clone()).ToList();
                    candidate[i].Words.RemoveAt(j);
                    if (candidate[i].Words.Count == 0) candidate.RemoveAt(i);
                    yield return (candidate, text);
                }
            }

            for (int i = 0; i < text.Length; i++)
                yield return (batches, text.Remove(i, 1));
        }

        /// <summary>
        /// Greedily removes words and characters for as long as the case still mismatches. Every step
        /// strictly reduces the size of the case, so this terminates.
        /// </summary>
        private static (List<Batch> Batches, string Text) Shrink(List<Batch> batches, string text)
        {
            bool shrunk = true;

            while (shrunk)
            {
                shrunk = false;

                foreach (var (candidateBatches, candidateText) in Smaller(batches, text))
                {
                    if (Mismatches(candidateBatches, candidateText))
                    {
                        (batches, text) = (candidateBatches, candidateText);
                        shrunk = true;
                        break;
                    }
                }
            }

            return (batches, text);
        }

        private static string Describe(List<Batch> batches, string text)
        {
            var plan = batches.Select(b => (b.ViaEnumerable ? "Add([" : "Add each + BuildFail([") + string.Join(", ", b.Words) + "])");
            return $"{string.Join(" then ", plan)}, Search(\"{text}\")"
                + $"{Environment.NewLine}actual:   {string.Join(", ", Normalize(TrieSearch(batches, text)))}"
                + $"{Environment.NewLine}expected: {string.Join(", ", Normalize(BruteForceSearch(batches, text)))}";
        }

        private static void AssertMatchesBruteForce(int seed, Func<Random, List<Batch>> createBatches)
        {
            var random = new Random(seed);

            for (int i = 0; i < CasesPerSeed; i++)
            {
                var batches = createBatches(random);
                var text = RandomString(random, 10);

                if (Mismatches(batches, text))
                {
                    var (shrunkBatches, shrunkText) = Shrink(batches, text);
                    Assert.Fail($"Search does not agree with a brute force search: {Describe(shrunkBatches, shrunkText)}");
                }
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void SearchMatchesBruteForceTest(int seed)
        {
            AssertMatchesBruteForce(seed, random =>
                [new Batch { Words = RandomWords(random, 6, 4), ViaEnumerable = true }]);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void IncrementalAddMatchesBruteForceTest(int seed)
        {
            // Words added after the trie was built can give a node a longer failure link than the one it
            // got on an earlier build, which used to be missed and lost matches of the words added last.
            AssertMatchesBruteForce(seed, random =>
                [.. Enumerable.Range(0, random.Next(2, 5)).Select(_ => new Batch
                {
                    Words = RandomWords(random, 3, 4),
                    ViaEnumerable = random.Next(2) == 0,
                })]);
        }
    }
}
