﻿using System.Collections.Generic;

namespace Ganss.Text
{
    /// <summary>
    /// Provides extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether this instance contains the specified words.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="words">The words.</param>
        /// <returns>The matched words.</returns>
        public static IEnumerable<WordMatch> Contains(this string text, IEnumerable<string> words)
        {
            return new AhoCorasick(words).Search(text);
        }

        /// <summary>
        /// Determines whether this instance contains the specified words.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="words">The words.</param>
        /// <returns>The matched words.</returns>
        public static IEnumerable<WordMatch> Contains(this string text, params string[] words)
        {
            return new AhoCorasick(words).Search(text);
        }

        /// <summary>
        /// Determines whether this instance contains the specified words.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        /// <param name="words">The words.</param>
        /// <returns>The matched words.</returns>
        public static IEnumerable<WordMatch> Contains(this string text, IEqualityComparer<char> comparer, IEnumerable<string> words)
        {
            return new AhoCorasick(comparer, words).Search(text);
        }

        /// <summary>
        /// Determines whether this instance contains the specified words.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="comparer">The comparer used to compare individual characters.</param>
        /// <param name="words">The words.</param>
        /// <returns>The matched words.</returns>
        public static IEnumerable<WordMatch> Contains(this string text, IEqualityComparer<char> comparer, params string[] words)
        {
            return new AhoCorasick(comparer, words).Search(text);
        }
    }
}
