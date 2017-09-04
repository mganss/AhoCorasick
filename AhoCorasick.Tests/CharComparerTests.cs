using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ganss.Text.Tests
{
    public class CharComparerTests
    {
        const char SmallDotlessI = '\u0131';
        const char CapitalIWithDot = '\u0130';
        const char CapitalSharpS = '\u1e9e';
        const char LatinSmallCapitalR = '\u0280';
        const char LatinLetterYR = '\u01a6';

        [Test]
        public void OrdinalTest()
        {
            var c = CharComparer.Ordinal;
            Assert.IsTrue(c.Equals('i', 'i'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'i'));
            Assert.IsFalse(c.Equals('ß', CapitalSharpS));
            Assert.IsFalse(c.Equals(LatinSmallCapitalR, LatinLetterYR));

            c = CharComparer.OrdinalIgnoreCase;
            Assert.IsTrue(c.Equals('i', 'I'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'i'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'I'));
            Assert.IsFalse(c.Equals('ß', CapitalSharpS));
            Assert.IsTrue(c.Equals(LatinSmallCapitalR, LatinLetterYR));
        }

        [Test]
        public void InvariantTest()
        {
            var c = CharComparer.InvariantCulture;
            Assert.IsTrue(c.Equals('i', 'i'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'i'));
            Assert.IsFalse(c.Equals('ß', CapitalSharpS));
            Assert.IsFalse(c.Equals(LatinSmallCapitalR, LatinLetterYR));

            c = CharComparer.InvariantCultureIgnoreCase;
            Assert.IsTrue(c.Equals('i', 'I'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'i'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'I'));
            Assert.IsTrue(c.Equals('ß', CapitalSharpS));
            Assert.IsFalse(c.Equals(LatinSmallCapitalR, LatinLetterYR));
        }

        [Test]
        public void CultureTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            var c = CharComparer.CurrentCulture;
            Assert.IsTrue(c.Equals('i', 'i'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'i'));
            Assert.IsFalse(c.Equals('ß', CapitalSharpS));
            Assert.IsFalse(c.Equals(LatinSmallCapitalR, LatinLetterYR));

            c = CharComparer.CurrentCultureIgnoreCase;
            Assert.IsFalse(c.Equals('i', 'I'));
            Assert.IsFalse(c.Equals(SmallDotlessI, 'i'));
            Assert.IsTrue(c.Equals(SmallDotlessI, 'I'));
            Assert.IsTrue(c.Equals('i', CapitalIWithDot));
            Assert.IsTrue(c.Equals('ß', CapitalSharpS));
            Assert.IsFalse(c.Equals(LatinSmallCapitalR, LatinLetterYR));

            Assert.AreEqual(c.GetHashCode('i'), c.GetHashCode(CapitalIWithDot));
            Assert.AreEqual(c.GetHashCode(SmallDotlessI), c.GetHashCode('I'));
        }
    }
}
