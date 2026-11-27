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
        /// Whether words have been added since the failure nodes were last built.
        /// </summary>
        private bool needsBuildFail;

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
        public AhoCorasick()
        {
            Trie = new Trie();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AhoCorasick"/> class.
        /// </summary>
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
        /// <remarks>Does not build the failure nodes. <see cref="Search"/> builds them when needed,
        /// call <see cref="BuildFail"/> explicitly to control when that happens.</remarks>
        /// <param name="word">The word.</param>
        public void Add(string word)
        {
            // Set before adding, so that a word that fails to be added does not leave the
            // failure nodes marked as up to date.
            needsBuildFail = true;
            Trie.Add(word);
        }

        /// <summary>
        /// Adds the specified words and builds the failure nodes.
        /// </summary>
        /// <param name="words">The words.</param>
        public void Add(IEnumerable<string> words)
        {
            needsBuildFail = true;

            foreach (var word in words)
            {
                Trie.Add(word);
            }

            BuildFail();
        }

        /// <summary>
        /// Builds the failure nodes necessary to perform search.
        /// </summary>
        /// <remarks>Can be called repeatedly. Every failure node is recomputed, so words added since
        /// the last build are taken into account.</remarks>
        /// <param name="node">The start node. Pass <c>null</c> to build the whole trie.</param>
        public void BuildFail(Trie node = null)
        {
            var wholeTrie = node == null;

            node ??= Trie;

            // Words added since the last build can give a node a longer suffix match,
            // so recompute every failure link instead of keeping the old one.
            node.Fail = null;

            var word = node.Word;
            for (int i = 1; i < word.Length && node.Fail == null; i++)
                node.Fail = Trie.ExploreFailLink(word, i, word.Length);

            foreach (var subNode in node.Next.Values)
                BuildFail(subNode);

            // Only a build that started at the root leaves the whole trie searchable.
            if (wholeTrie) needsBuildFail = false;
        }

        /// <summary>
        /// Searches for words in the specified text.
        /// </summary>
        /// <remarks>Builds the failure nodes first if words have been added since they were last built.
        /// Because that modifies the trie, searching an instance that words were added to without
        /// calling <see cref="BuildFail"/> is not thread safe.</remarks>
        /// <param name="text">The text.</param>
        /// <returns>The matched words.</returns>
        public virtual IEnumerable<WordMatch> Search(string text)
        {
            // Not an iterator method, so this happens when Search is called rather than
            // when the result is first enumerated.
            if (needsBuildFail) BuildFail();

            return SearchBuilt(text);
        }

        /// <summary>
        /// Searches for words in the specified text, assuming the failure nodes have been built.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The matched words.</returns>
        private IEnumerable<WordMatch> SearchBuilt(string text)
        {
            if (text == null) yield break;

            var current = Trie;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                while (current != null && !current.Next.ContainsKey(c))
                    current = current.Fail;

                current ??= Trie;

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
