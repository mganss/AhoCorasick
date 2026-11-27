using System;
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
        /// Gets a value indicating whether words have been added since the failure nodes were last built.
        /// </summary>
        /// <value>
        ///   <c>true</c> if <see cref="Search"/> has to build the failure nodes before it can search;
        ///   otherwise, <c>false</c>.
        /// </value>
        public bool NeedsBuildFail { get; private set; }

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
            NeedsBuildFail = true;
            Trie.Add(word);
        }

        /// <summary>
        /// Adds the specified words and builds the failure nodes.
        /// </summary>
        /// <param name="words">The words.</param>
        public void Add(IEnumerable<string> words)
        {
            NeedsBuildFail = true;

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
        /// <param name="node">Ignored. The failure nodes are built for the whole trie, which is what
        /// makes them correct: a node can fail to a node in an unrelated branch of the trie.</param>
        public void BuildFail(Trie node = null)
        {
            // The failure node of a node is the node of the longest proper suffix of its word that is
            // in the trie. That suffix is a suffix of the node's parent's word extended by the node's
            // last character, so the failure node can be found by following the failure nodes of the
            // parent, provided those are known already. Hence breadth first, by increasing word length.
            var queue = new Queue<Trie>();

            Trie.Fail = null;
            Trie.WordFail = null;

            foreach (var child in Trie.Next.Values)
            {
                // A one character word has no proper suffix other than the empty one, which is the
                // root, represented by a null failure node.
                child.Fail = null;
                child.WordFail = null;
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var next in current.Next)
                {
                    LinkFailNodes(current, next.Key, next.Value);
                    queue.Enqueue(next.Value);
                }
            }

            NeedsBuildFail = false;
        }

        /// <summary>
        /// Sets the failure node of a child node, and the nearest word on its failure chain.
        /// </summary>
        /// <param name="parent">The parent node, whose failure node must be known already.</param>
        /// <param name="c">The character the parent maps to the child by.</param>
        /// <param name="child">The child node.</param>
        private void LinkFailNodes(Trie parent, char c, Trie child)
        {
            // The longest suffix of the parent's word that can be extended by c.
            var fail = parent.Fail;

            while (fail != null && !fail.Next.ContainsKey(c))
                fail = fail.Fail;

            child.Fail = (fail ?? Trie).Next.TryGetValue(c, out var suffix) ? suffix : null;

            // The nearest word on the failure chain, so that Search does not have to walk the whole
            // chain to find the words ending at a position.
            if (child.Fail == null)
                child.WordFail = null;
            else if (child.Fail.IsWord)
                child.WordFail = child.Fail;
            else
                child.WordFail = child.Fail.WordFail;
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
            if (NeedsBuildFail) BuildFail();

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
                    // Every word ending here is the current node, if it is a word, followed by the
                    // words on its failure chain, which WordFail links together directly.
                    var node = current.IsWord ? current : current.WordFail;

                    while (node != null)
                    {
                        var word = node.Word;
                        var offset = i + 1 - word.Length;
                        yield return new WordMatch { Index = offset, Word = word };

                        node = node.WordFail;
                    }
                }
            }
        }
    }
}
