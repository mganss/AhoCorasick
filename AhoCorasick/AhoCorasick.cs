using System.Collections.Generic;

namespace Ganss.Text
{
    /// <summary>
    /// Represents a word match.
    /// </summary>
    public struct WordMatch
    {
        /// <summary>
        /// Gets or sets the index of the matched word in the searched text string.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the matched word.
        /// </summary>
        /// <value>
        /// The matched word.
        /// </value>
        public string Word { get; set; }
    }

    /// <summary>
    /// Implements the <a href="https://en.wikipedia.org/wiki/Aho%E2%80%93Corasick_string_matching_algorithm">Aho-Corasick algorithm</a>.
    /// </summary>
    public class AhoCorasick
    {
        /// <summary>
        /// Gets or sets the trie.
        /// </summary>
        /// <value>
        /// The trie.
        /// </value>
        protected Trie Trie { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// <remarks>Does not build the failure nodes. Call <see cref="BuildFail"/> after adding words before calling <see cref="Search"/>.</remarks>
        /// </summary>
        public AhoCorasick()
        {
            Trie = new Trie();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
        /// <remarks>Does not build the failure nodes. Call <see cref="BuildFail"/> after adding words before calling <see cref="Search"/>.</remarks>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        public AhoCorasick(IEqualityComparer<char> comparer)
        {
            Trie = new Trie(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
        /// <param name="words">The words to find.</param>
        public AhoCorasick(params string[] words)
            : this()
        {
            Add(words);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
        /// <param name="words">The words to find.</param>
        public AhoCorasick(IEnumerable<string> words)
            : this()
        {
            Add(words);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        /// <param name="words">The words to find.</param>
        public AhoCorasick(IEqualityComparer<char> comparer, params string[] words)
            : this(comparer)
        {
            Add(words);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        /// <param name="words">The words to find.</param>
        public AhoCorasick(IEqualityComparer<char> comparer, IEnumerable<string> words)
            : this(comparer)
        {
            Add(words);
        }

        /// <summary>
        /// Adds the specified word.
        /// </summary>
        /// <remarks>Does not build the failure nodes. Call <see cref="BuildFail"/> after adding words before calling <see cref="Search"/>.</remarks>
        /// <param name="word">The word.</param>
        public void Add(string word)
        {
            Trie.Add(word);
        }

        /// <summary>
        /// Adds the specified words.
        /// </summary>
        /// <param name="words">The words.</param>
        public void Add(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                Trie.Add(word);
            }

            BuildFail();
        }

        /// <summary>
        /// Builds the failure nodes necessary to perform search.
        /// </summary>
        /// <param name="node">The start node.</param>
        public void BuildFail(Trie node = null)
        {
            node = node ?? Trie;

            var word = node.Word;
            for (int i = 1; i < word.Length && node.Fail == null; i++)
                node.Fail = Trie.ExploreFailLink(word.Substring(i));

            foreach (var subNode in node.Next.Values)
                BuildFail(subNode);
        }

        /// <summary>
        /// Searches for words in the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The matched words.</returns>
        public virtual IEnumerable<WordMatch> Search(string text)
        {
            if (text == null) yield break;

            var current = Trie;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                while (current != null && !current.Next.ContainsKey(c))
                    current = current.Fail;

                if (current == null) current = Trie;

                if (current.Next.TryGetValue(c, out current))
                {
                    var node = current;

                    while (node != null)
                    {
                        if (node.IsWord)
                        {
                            var word = node.Word;
                            var offset = i + 1 - word.Length;
                            yield return new WordMatch { Index = offset, Word = word };
                        }

                        node = node.Fail;
                    }
                }
            }
        }
    }
}
