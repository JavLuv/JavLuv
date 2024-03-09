using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace JavLuv
{
    public class StringSanitizeTest
    {
        #region Public Functions

        [Fact]
        public void TestCleanEnglish()
        {
            String s1 = "Simple Test";
            String s2 = s1.Sanitize();
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void TestCleanJapanese()
        {
            String s1 = "簡単なテスト";
            String s2 = s1.Sanitize();
            Assert.Equal(s1, s2);
        }

        [Fact]
        public void TestCase1()
        {
            String s1 = "abcdn\u200b";
            String s2 = s1.Sanitize();
            Assert.Equal("abcd", s2);
        }

        #endregion
    }
}
