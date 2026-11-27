using System;
using System.Collections.Generic;

namespace Ganss.Text
{
    /// <summary>
    /// A <a href="https://en.wikipedia.org/wiki/Trie">Trie</a>.
    /// </summary>
    public class Trie
    {
        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>
        /// The child nodes.
        /// </value>
        public Dictionary<char, Trie> Next { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance represents a word in the dictionary.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a word in the dictionary; otherwise, <c>false</c>.
        /// </value>
        public bool IsWord { get; set; }

        /// <summary>
        /// Gets or sets the failure node.
        /// </summary>
        /// <value>
        /// The failure node.
        /// </value>
        public Trie Fail { get; set; }

        /// <summary>
        /// Gets or sets the nearest node on the failure chain that represents a word, if any.
        /// </summary>
        /// <value>
        /// The nearest failure node that is a word.
        /// </value>
        /// <remarks>Lets a search enumerate the words ending at a position without walking the whole
        /// failure chain. Internal because it is only meaningful while the failure nodes are built.</remarks>
        internal Trie WordFail { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        /// <value>
        /// The parent node.
        /// </value>
        public Trie Parent { get; set; }

        /// <summary>
        /// Gets the word prefix this node represents.
        /// </summary>
        /// <value>
        /// The word prefix.
        /// </value>
        public string Word { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie"/> class.
        /// </summary>
        public Trie()
        {
            Word = "";
            Next = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie"/> class.
        /// </summary>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        public Trie(IEqualityComparer<char> comparer)
        {
            Word = "";
            Next = new Dictionary<char, Trie>(comparer);
        }

        /// <summary>
        /// Adds the specified word to the trie.
        /// </summary>
        /// <remarks>Walks the word instead of calling itself once per character, so that the length of
        /// a word is not limited by the stack.</remarks>
        /// <param name="word">The word.</param>
        /// <returns>The node of the added word.</returns>
        public virtual Trie Add(string word)
        {
            // Indexed before the loop so that null and the empty string still throw what they threw
            // when this method recursed.
            _ = word[0];

            var node = this;

            for (int i = 0; i < word.Length; i++)
            {
                var c = word[i];

                if (!node.Next.TryGetValue(c, out Trie next))
                    node.Next[c] = next = new Trie(node.Next.Comparer) { Parent = node, Word = node.Word + c };

                node = next;
            }

            node.IsWord = true;

            return node;
        }

        /// <summary>
        /// Finds the failure node for a specified suffix within the given range of indices.
        /// </summary>
        /// <param name="word">The string containing the suffix.</param>
        /// <param name="startIndex">The start index of the suffix within the string.</param>
        /// <param name="endIndex">The end index (exclusive) of the suffix within the string.</param>
        /// <returns>The failure node or null if no failure node is found.</returns>
        /// <remarks>No longer used. <see cref="AhoCorasick.BuildFail"/> builds the failure nodes breadth
        /// first from the failure nodes of the parents instead of exploring suffixes from the root, so
        /// overriding this method no longer has an effect on a search.</remarks>
        [Obsolete("Failure nodes are no longer explored from the root, so this method is no longer called. It will be removed in a future major version.")]
        public virtual Trie ExploreFailLink(string word, int startIndex, int endIndex)
        {
            var node = this;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (!node.Next.TryGetValue(word[i], out node))
                {
                    return null;
                }
            }

            return node;
        }
    }
}
