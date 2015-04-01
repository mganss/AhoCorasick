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
        /// Gets or sets the character this node represents.
        /// </summary>
        /// <value>
        /// The character this node represents.
        /// </value>
        public char Char { get; set; }

        /// <summary>
        /// Gets or sets the failure node.
        /// </summary>
        /// <value>
        /// The failure node.
        /// </value>
        public Trie Fail { get; set; }

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
        public string Word
        {
            get
            {
                return Parent == null ? "" : (Parent.Word + Char);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie"/> class.
        /// </summary>
        public Trie()
        {
            Next = new Dictionary<char, Trie>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie"/> class.
        /// </summary>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        public Trie(IEqualityComparer<char> comparer)
        {
            Next = new Dictionary<char, Trie>(comparer);
        }

        /// <summary>
        /// Adds the specified word to the trie.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public virtual Trie Add(string word)
        {
            var c = word[0];
            Trie node;

            if (!Next.TryGetValue(c, out node))
                Next[c] = node = new Trie(Next.Comparer) { Parent = this, Char = c };

            if (word.Length > 1)
                return node.Add(word.Substring(1));
            else
                node.IsWord = true;

            return node;
        }

        /// <summary>
        /// Finds the failure node for a specified suffix.
        /// </summary>
        /// <param name="word">The suffix.</param>
        /// <returns>The failure node or null.</returns>
        public virtual Trie ExploreFailLink(string word)
        {
            var node = this;

            foreach (var c in word)
            {
                node.Next.TryGetValue(c, out node);
                if (node == null) return null;
            }

            return node;
        }
    }
}
