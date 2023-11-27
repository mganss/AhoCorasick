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
            Assert.That(c.Equals('i', 'i'), Is.True);
            Assert.That(c.Equals(SmallDotlessI, 'i'), Is.False);
            Assert.That(c.Equals('ß', CapitalSharpS), Is.False);
            Assert.That(c.Equals(LatinSmallCapitalR, LatinLetterYR), Is.False);

            c = CharComparer.OrdinalIgnoreCase;
            Assert.That(c.Equals('i', 'I'), Is.True);
            Assert.That(c.Equals(SmallDotlessI, 'i'), Is.False);
            Assert.That(c.Equals(SmallDotlessI, 'I'), Is.False);
            Assert.That(c.Equals('ß', CapitalSharpS), Is.False);
            Assert.That(c.Equals(LatinSmallCapitalR, LatinLetterYR), Is.True);
        }

        [Test]
        public void InvariantTest()
        {
            var c = CharComparer.InvariantCulture;
            Assert.That(c.Equals('i', 'i'), Is.True);
            Assert.That(c.Equals(SmallDotlessI, 'i'), Is.False);
            Assert.That(c.Equals('ß', CapitalSharpS), Is.False);
            Assert.That(c.Equals(LatinSmallCapitalR, LatinLetterYR), Is.False);

            c = CharComparer.InvariantCultureIgnoreCase;
            Assert.That(c.Equals('i', 'I'), Is.True);
            Assert.That(c.Equals(SmallDotlessI, 'i'), Is.False);
            Assert.That(c.Equals(SmallDotlessI, 'I'), Is.False);
            Assert.That(c.Equals('ß', CapitalSharpS), Is.True);
        }

        [Test]
        public void CultureTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            var c = CharComparer.CurrentCulture;
            Assert.That(c.Equals('i', 'i'), Is.True);
            Assert.That(c.Equals(SmallDotlessI, 'i'), Is.False);
            Assert.That(c.Equals('ß', CapitalSharpS), Is.False);
            Assert.That(c.Equals(LatinSmallCapitalR, LatinLetterYR), Is.False);

            c = CharComparer.CurrentCultureIgnoreCase;
            Assert.That(c.Equals('i', 'I'), Is.False);
            Assert.That(c.Equals(SmallDotlessI, 'i'), Is.False);
            Assert.That(c.Equals(SmallDotlessI, 'I'), Is.True);
            Assert.That(c.Equals('i', CapitalIWithDot), Is.True);
            Assert.That(c.Equals('ß', CapitalSharpS), Is.True);

            Assert.That(c.GetHashCode('i'), Is.EqualTo(c.GetHashCode(CapitalIWithDot)));
            Assert.That(c.GetHashCode(SmallDotlessI), Is.EqualTo(c.GetHashCode('I')));
        }
    }
}
