using Common;
using MovieInfo;
using WebScraper;

namespace JavLuv
{
    public class ScrapeActressTest
    {
        [Fact]
        public void TestScrapeActressJavDatabase()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavDatabase(actressData.Name, LanguageType.English);
            var scraper = new Scraper();
            actressData = scraper.ScrapeActress(module, actressData);
            Assert.Equal("Yui Hatano", actressData.Name);
            Assert.Equal("波多野結衣", actressData.JapaneseName);
            Assert.Equal(1988, actressData.DobYear);
            Assert.Equal(5, actressData.DobMonth);
            Assert.Equal(24, actressData.DobDay);
            Assert.Equal(163, actressData.Height);
            Assert.Equal("E", actressData.Cup);
            Assert.Equal(86, actressData.Bust);
            Assert.Equal(59, actressData.Waist);
            Assert.Equal(94, actressData.Hips);
            Assert.Equal("A", actressData.BloodType);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeActressJavModel()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavModel(actressData.Name, LanguageType.English);
            var scraper = new Scraper();
            actressData = scraper.ScrapeActress(module, actressData);
            Assert.Equal("Yui Hatano", actressData.Name);
            Assert.Equal("波多野結衣", actressData.JapaneseName);
            Assert.Equal(1988, actressData.DobYear);
            Assert.Equal(5, actressData.DobMonth);
            Assert.Equal(24, actressData.DobDay);
            Assert.Equal(163, actressData.Height);
            Assert.Equal(88, actressData.Bust);
            Assert.Equal(59, actressData.Waist);
            Assert.Equal(85, actressData.Hips);
            Assert.Equal("A", actressData.BloodType);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeActressJavRaveClub1()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavRaveClub(actressData.Name, LanguageType.English);
            var scraper = new Scraper();
            actressData = scraper.ScrapeActress(module, actressData);
            Assert.Equal("Yui Hatano", actressData.Name);
            Assert.Equal("波多野結衣", actressData.JapaneseName);
            Assert.Equal(1988, actressData.DobYear);
            Assert.Equal(5, actressData.DobMonth);
            Assert.Equal(24, actressData.DobDay);
            Assert.Equal(163, actressData.Height);
            Assert.Equal("E", actressData.Cup);
            Assert.Equal(88, actressData.Bust);
            Assert.Equal(59, actressData.Waist);
            Assert.Equal(85, actressData.Hips);
            Assert.Equal("A", actressData.BloodType);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeActressJavRaveClub2()
        {
            var actressData = new ActressData("Asami Mizuhata");
            var module = new ActressJavRaveClub(actressData.Name, LanguageType.English);
            var scraper = new Scraper();
            actressData = scraper.ScrapeActress(module, actressData);
            Assert.Equal("Asami Mizuhata", actressData.Name);
            Assert.Equal("水端あさみ", actressData.JapaneseName);
            Assert.Equal(1990, actressData.DobYear);
            Assert.Equal(1, actressData.DobMonth);
            Assert.Equal(30, actressData.DobDay);
            Assert.Equal(165, actressData.Height);
            Assert.Equal("D", actressData.Cup);
            Assert.Equal(85, actressData.Bust);
            Assert.Equal(58, actressData.Waist);
            Assert.Equal(88, actressData.Hips);
            Assert.Equal("", actressData.BloodType);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeActressJavRaveClub3()
        {
            var actressData = new ActressData("Moe Ooishi");
            var module = new ActressJavRaveClub(actressData.Name, LanguageType.English);
            var scraper = new Scraper();
            actressData = scraper.ScrapeActress(module, actressData);
            Assert.Equal("Moe Ooishi", actressData.Name);
            Assert.Equal("大石もえ", actressData.JapaneseName);
            Assert.Equal(1984, actressData.DobYear);
            Assert.Equal(12, actressData.DobMonth);
            Assert.Equal(11, actressData.DobDay);
            Assert.Equal(170, actressData.Height);
            Assert.Equal("F", actressData.Cup);
            Assert.Equal(96, actressData.Bust);
            Assert.Equal(64, actressData.Waist);
            Assert.Equal(88, actressData.Hips);
            Assert.Equal("A", actressData.BloodType);
            Assert.NotEmpty(module.ImageSource);
        }

        /*
        [Fact]
        public void TestScrapeActressJavBody()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavBody(actressData.Name, LanguageType.English);
            TestActressScraper(actressData, module);
        }

        */

    }
}
