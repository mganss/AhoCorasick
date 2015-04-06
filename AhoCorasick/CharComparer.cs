using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ganss.Text
{
    class OrdinalCharComparer : CharComparer
    {
        private bool _ignoreCase;

        public OrdinalCharComparer(bool ignoreCase = false)
        {
            _ignoreCase = ignoreCase;
        }

        public override bool Equals(char x, char y)
        {
            return _ignoreCase ? ((uint)char.ToUpperInvariant(x)).Equals(((uint)char.ToUpperInvariant(y)))
                : ((uint)x).Equals((uint)y);
        }

        public override int GetHashCode(char obj)
        {
            return _ignoreCase ? (int)char.ToUpperInvariant(obj) : (int)obj;
        }
    }

    class CultureCharComparer: CharComparer
    {
        private StringComparer _stringComparer;

        public CultureCharComparer(CultureInfo cultureInfo, bool ignoreCase = false)
        {
            _stringComparer = StringComparer.Create(cultureInfo, ignoreCase);
        }

        public override bool Equals(char x, char y)
        {
            return _stringComparer.Equals(x.ToString(), y.ToString());
        }

        public override int GetHashCode(char obj)
        {
            return _stringComparer.GetHashCode(obj.ToString());
        }
    }

    /// <summary>
    /// Represents a char comparison operation that uses specific case and culture-based or ordinal comparison rules.
    /// </summary>
    public abstract class CharComparer: EqualityComparer<char>
    {
        private static readonly CharComparer _ordinalIgnoreCase = new OrdinalCharComparer(ignoreCase: true);

        /// <summary>
        /// Gets a <see cref="CharComparer"/> object that performs a case-insensitive ordinal comparison.
        /// </summary>
        /// <value>
        /// A <see cref="CharComparer"/> object.
        /// </value>
        public static CharComparer OrdinalIgnoreCase
        {
            get
            {
                return _ordinalIgnoreCase;
            }
        }

        private static readonly CharComparer _ordinal = new OrdinalCharComparer(ignoreCase: false);

        /// <summary>
        /// Gets a <see cref="CharComparer"/> object that performs a case-sensitive ordinal comparison.
        /// </summary>
        /// <value>
        /// A <see cref="CharComparer"/> object.
        /// </value>
        public static CharComparer Ordinal
        {
            get
            {
                return _ordinal;
            }
        }

        private static readonly CharComparer _invariantCultureIgnoreCase = new CultureCharComparer(CultureInfo.InvariantCulture, ignoreCase: true);

        /// <summary>
        /// Gets a <see cref="CharComparer"/> object that performs a case-insensitive comparison using the comparison rules of the invariant culture.
        /// </summary>
        /// <value>
        /// A <see cref="CharComparer"/> object.
        /// </value>
        public static CharComparer InvariantCultureIgnoreCase
        {
            get
            {
                return _invariantCultureIgnoreCase;
            }
        }

        private static readonly CharComparer _invariantCulture = new CultureCharComparer(CultureInfo.InvariantCulture, ignoreCase: false);

        /// <summary>
        /// Gets a <see cref="CharComparer"/> object that performs a case-sensitive comparison using the comparison rules of the invariant culture.
        /// </summary>
        /// <value>
        /// A <see cref="CharComparer"/> object.
        /// </value>
        public static CharComparer InvariantCulture
        {
            get
            {
                return _invariantCulture;
            }
        }

        /// <summary>
        /// Gets a <see cref="CharComparer"/> object that performs a case-sensitive comparison using the comparison rules of the current culture.
        /// </summary>
        /// <value>
        /// A <see cref="CharComparer"/> object.
        /// </value>
        public static CharComparer CurrentCulture
        {
            get
            {
                return new CultureCharComparer(CultureInfo.CurrentCulture, ignoreCase: false);
            }
        }

        /// <summary>
        /// Gets a <see cref="CharComparer"/> object that performs a case-insensitive comparison using the comparison rules of the current culture.
        /// </summary>
        /// <value>
        /// A <see cref="CharComparer"/> object.
        /// </value>
        public static CharComparer CurrentCultureIgnoreCase
        {
            get
            {
                return new CultureCharComparer(CultureInfo.CurrentCulture, ignoreCase: true);
            }
        }

        /// <summary>
        /// Creates a <see cref="CharComparer"/> object that compares characters according to the rules of a specified culture.
        /// </summary>
        /// <param name="cultureInfo">A culture whose linguistic rules are used to perform a string comparison.</param>
        /// <param name="ignoreCase">true to specify that comparison operations be case-insensitive; false to specify that comparison operations be case-sensitive.</param>
        /// <returns>A new <see cref="CharComparer"/> object that performs character comparisons according to the comparison rules used by the <paramref name="cultureInfo"/> parameter and the case rule specified by the <paramref name="ignoreCase"/> parameter.</returns>
        public static CharComparer Create(CultureInfo cultureInfo, bool ignoreCase)
        {
            return new CultureCharComparer(cultureInfo, ignoreCase);
        }
    }
}
