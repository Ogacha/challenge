using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace UnitTest
{
    [TestClass]
    public class ForRegex
    {
        Regex symbolRegex = new Regex(@"^([A-Z0-9.]+)(,[A-Z0-9.]+)*$");

        [DataTestMethod,
            DataRow("ABC0,DEF,G"),
            DataRow("OP,0.123"),
            DataRow("AGE")]
        public void RegexOK(string symbol)
        {
            var actual = symbolRegex.IsMatch(symbol);
            Assert.IsTrue(actual);
        }

        [DataTestMethod,
            DataRow("ABC0,DEF,G,"),
            DataRow("OP@,ee3"),
            DataRow("P&G,AGE"),
            DataRow(",A,AG"),
            DataRow("ab,ee")]
        public void RegexError(string symbol)
        {
            var actual = symbolRegex.IsMatch(symbol);
            Assert.IsFalse(actual);
        }
    }
}
