using Common;
using MovieInfo;
using WebScraper;

namespace JavLuv
{
    public class ScrapeMovieTest
    {
        [Fact]
        public void TestScrapeMovieJavLibrary()
        {
            var metadata = new MovieMetadata("IESP-711");
            var module = new MovieJavLibrary(metadata, LanguageType.English);
            module.Scrape();
            Assert.Equal("Minami Iroha Lesbian Ban - I Was Dragged Into A Lesbian Swamp By My Sister-In-Law -", module.Metadata.Title);
            Assert.Equal("2022-12-22", module.Metadata.Premiered);
            Assert.Equal(140, module.Metadata.Runtime);
            Assert.Equal("Rin Kiku", module.Metadata.Director);
            Assert.Equal("IE NERGY", module.Metadata.Studio);
            Assert.Equal("Iesp", module.Metadata.Label);
            Assert.Equal(4, module.Metadata.Genres.Count);
            Assert.Equal("Lesbian", module.Metadata.Genres[0]);
            Assert.Equal("Big Tits", module.Metadata.Genres[1]);
            Assert.Equal("Slender", module.Metadata.Genres[2]);
            Assert.Equal("Drama", module.Metadata.Genres[3]);
            Assert.Equal(2, module.Metadata.Actors.Count);
            Assert.Equal("Rika Aimi", module.Metadata.Actors[0].Name);
            Assert.Equal("Iroha Minami", module.Metadata.Actors[1].Name);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeMovieJavDatabase()
        {
            var metadata = new MovieMetadata("IESP-711");
            var module = new MovieJavDatabase(metadata, LanguageType.English);
            module.Scrape();
            Assert.Equal("Minami Iroha Lesbian Ban - I Was Dragged Into A Lesbian Swamp By My Sister-In-Law -", module.Metadata.Title);
            Assert.Equal("2022-12-22", module.Metadata.Premiered);
            Assert.Equal(142, module.Metadata.Runtime);
            Assert.Equal("chrysanthemum", module.Metadata.Director);
            Assert.Equal("Ienergy", module.Metadata.Studio);
            Assert.Equal(5, module.Metadata.Genres.Count);
            Assert.Equal("Big Tits", module.Metadata.Genres[0]);
            Assert.Equal("Drama", module.Metadata.Genres[1]);
            Assert.Equal("Hi-Def", module.Metadata.Genres[2]);
            Assert.Equal("Lesbian", module.Metadata.Genres[3]);
            Assert.Equal("Slender", module.Metadata.Genres[4]);
            Assert.Equal(2, module.Metadata.Actors.Count);
            Assert.Equal("Iroha Minami", module.Metadata.Actors[0].Name);
            Assert.Equal("Rika Omi", module.Metadata.Actors[1].Name);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeMovieJavLand()
        {
            var metadata = new MovieMetadata("IESP-711");
            var module = new MovieJavLand(metadata, LanguageType.English);
            module.Scrape();
            Assert.Equal("Minami Iroha Lesbian Ban Lifted ~I Was Dragged Into A Lesbian Swamp By My Sister-In-Law ~", module.Metadata.Title);
            Assert.Equal("2022-12-22", module.Metadata.Premiered);
            Assert.Equal(142, module.Metadata.Runtime);
            Assert.Equal("Chrysanthemum", module.Metadata.Director);
            Assert.Equal("I-Energy", module.Metadata.Studio);
            Assert.Equal("Iesp", module.Metadata.Label);
            Assert.Equal(5, module.Metadata.Genres.Count);
            Assert.Equal("High Vision", module.Metadata.Genres[0]);
            Assert.Equal("Drama", module.Metadata.Genres[1]);
            Assert.Equal("Big Tits", module.Metadata.Genres[2]);
            Assert.Equal("Slender", module.Metadata.Genres[3]);
            Assert.Equal("Lesbian", module.Metadata.Genres[4]);
            Assert.Equal(2, module.Metadata.Actors.Count);
            Assert.Equal("Iroha Minami", module.Metadata.Actors[0].Name);
            Assert.Equal("Tadami Rumi", module.Metadata.Actors[1].Name);
            Assert.NotEmpty(module.ImageSource);
        }

        [Fact]
        public void TestScrapeMovieJavSeenTv1()
        {
            var metadata = new MovieMetadata("HAVD-959");
            var module = new MovieJavSeenTv(metadata, LanguageType.English);
            module.Scrape();
            Assert.Equal("- Kiss Kiss Of Ladies Lesbian Lesbian Lesbian Lesbian Secret Affair - Hibino", module.Metadata.Title);          
            Assert.Empty(module.Metadata.Premiered);
            Assert.Equal(0, module.Metadata.Runtime);
            Assert.Equal("Dr.TORIHAMA", module.Metadata.Director);
            Assert.Equal("Hibino", module.Metadata.Studio);
            Assert.Empty(module.Metadata.Label);
        }

        [Fact]
        public void TestScrapeMovieJavSeenTv2()
        {
            var metadata = new MovieMetadata("BBAN-333");
            var module = new MovieJavSeenTv(metadata, LanguageType.English);
            module.Scrape();
            Assert.Equal("Who Is She...? A Barely Legal Suddenly Targeted And Fucked - Aoi Nakajo, Mio Ichijo", module.Metadata.Title);
            Assert.Equal("2021-7-2", module.Metadata.Premiered);
            Assert.Equal(0, module.Metadata.Runtime);
            Assert.Empty(module.Metadata.Director);
            Assert.Equal("bibian", module.Metadata.Studio);
            Assert.Equal("bibian", module.Metadata.Label);
        }

    }
}
