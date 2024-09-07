using Common;

namespace JavLuv
{
    public class IDParseUnitTest
    {
        #region Public Functions

        [Fact]
        public void TestBasic1()
        {
            string id = Utilities.ParseMovieID("ABC-123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBasic2()
        {
            string id = Utilities.ParseMovieID("ABC_123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBasic3()
        {
            string id = Utilities.ParseMovieID("ABC123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBasic4()
        {
            string id = Utilities.ParseMovieID("abc-123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBasic5()
        {
            string id = Utilities.ParseMovieID("abc_123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBasic6()
        {
            string id = Utilities.ParseMovieID("abc123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBasic7()
        {
            string id = Utilities.ParseMovieID("abc-1");
            Assert.Equal("ABC-1", id);
        }

        [Fact]
        public void TestBasic8()
        {
            string id = Utilities.ParseMovieID("abc 123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets1()
        {
            string id = Utilities.ParseMovieID("[ABC-123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets2()
        {
            string id = Utilities.ParseMovieID("[ABC_123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets3()
        {
            string id = Utilities.ParseMovieID("[ABC123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets4()
        {
            string id = Utilities.ParseMovieID("[abc-123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets5()
        {
            string id = Utilities.ParseMovieID("[abc_123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets6()
        {
            string id = Utilities.ParseMovieID("[abc123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestBrackets7()
        {
            string id = Utilities.ParseMovieID("[abc 123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestEndsWithD1()
        {
            string id = Utilities.ParseMovieID("abc 123d");
            Assert.Equal("ABC-123D", id);
        }

        [Fact]
        public void TestEndsWithD2()
        {
            string id = Utilities.ParseMovieID("[abc12-123d]");
            Assert.Equal("ABC12-123D", id);
        }

        [Fact]
        public void TestLong1()
        {
            string id = Utilities.ParseMovieID("ABCDEFG-12345");
            Assert.Equal("ABCDEFG-12345", id);
        }

        [Fact]
        public void TestLong2()
        {
            string id = Utilities.ParseMovieID("test test ABCDEFG-12345 test test");
            Assert.Equal("ABCDEFG-12345", id);
        }

        [Fact]
        public void TestLong3()
        {
            string id = Utilities.ParseMovieID("test test [ABCDEFG-12345] test test");
            Assert.Equal("ABCDEFG-12345", id);
        }

        [Fact]
        public void TestEmbedded1()
        {
            string id = Utilities.ParseMovieID("[abc-123]test");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestEmbedded2()
        {
            string id = Utilities.ParseMovieID("test[abc-123]test");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestMixed1()
        {
            string id = Utilities.ParseMovieID("abc12-123");
            Assert.Equal("ABC12-123", id);
        }

        [Fact]
        public void TestFalsePositive1()
        {
            string id = Utilities.ParseMovieID("Test at 90% test [ABC-123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestFalsePositive2()
        {
            string id = Utilities.ParseMovieID(" askfjkl3234 jksdfjk23 abd234 [ABC-123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestFalsePositive3()
        {
            string id = Utilities.ParseMovieID("abc-123 askfjkl3234 jksdfjk23 abd234");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestFalsePositive4()
        {
            string id = Utilities.ParseMovieID("abc123 askfjkl3234 jksdfjk23 abd234");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestFalsePositive5()
        {
            string id = Utilities.ParseMovieID("askfjkl3234 jksdfjk23 abd234 abc-123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestT28_1()
        {
            string id = Utilities.ParseMovieID("t28_494");
            Assert.Equal("T28-494", id);
        }

        [Fact]
        public void TestT28_2()
        {
            string id = Utilities.ParseMovieID("t28-494");
            Assert.Equal("T28-494", id);
        }

        [Fact]
        public void TestT28_3()
        {
            string id = Utilities.ParseMovieID("[T28-494]");
            Assert.Equal("T28-494", id);
        }

        [Fact]
        public void TestT28_4()
        {
            string id = Utilities.ParseMovieID("t28 494");
            Assert.Equal("T28-494", id);
        }

        [Fact]
        public void TestT28_5()
        {
            string id = Utilities.ParseMovieID("t28494");
            Assert.Equal("T28-494", id);
        }

        [Fact]
        public void TestT28_6()
        {
            string id = Utilities.ParseMovieID("FC2-PPV-1234567");
            Assert.Equal("FC2-PPV-1234567", id);
        }

        [Fact]
        public void TestFC2PPV_1()
        {
            string id = Utilities.ParseMovieID("FC2-PPV 12");
            Assert.Equal("FC2-PPV-12", id);
        }

        [Fact]
        public void TestFC2PPV_2()
        {
            string id = Utilities.ParseMovieID("FC2-PPV_12345");
            Assert.Equal("FC2-PPV-12345", id);
        }

        [Fact]
        public void TestFC2PPV_3()
        {
            string id = Utilities.ParseMovieID("FC2-PPV1234567");
            Assert.Equal("FC2-PPV-1234567", id);
        }

        [Fact]
        public void TestFC2PPV_4()
        {
            string id = Utilities.ParseMovieID("[Fc2-Ppv-12345678]");
            Assert.Equal("FC2-PPV-12345678", id);
        }

        [Fact]
        public void TestFC2PPV_5()
        {
            string id = Utilities.ParseMovieID("fasdf Fc2-Ppv-12345678 asdf");
            Assert.Equal("FC2-PPV-12345678", id);
        }

        [Fact]
        public void TestDMM_1()
        {
            string id = Utilities.ParseMovieID("ABC00123");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestDMM_2()
        {
            string id = Utilities.ParseMovieID("[ABC00123]");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestDMM_3()
        {
            string id = Utilities.ParseMovieID("00ABC00123xx");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestDMM_4()
        {
            string id = Utilities.ParseMovieID("00[ABC00123]xx");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestDMM_5()
        {
            string id = Utilities.ParseMovieID("  ABC00123  ");
            Assert.Equal("ABC-123", id);
        }

        [Fact]
        public void TestDMM_6()
        {
            string id = Utilities.ParseMovieID("abc00123 askfjkl3234 jksdfjk23 abd234");
            Assert.Equal("ABC-123", id);
        }

        #endregion
    }
}