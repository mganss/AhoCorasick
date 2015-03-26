using System.Collections.Generic;

namespace Ganss.Text
{
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
    }
}
