﻿using Common;
using MovieInfo;
using System;
using System.Threading;
using System.Windows.Threading;

namespace WebScraper
{
    public static class TestScraper
    {
        public static void RunTests(Dispatcher dispatcher, WebBrowser webBrowser)
        {
            m_dispatcher = dispatcher;
            m_webBrowser = webBrowser;
            m_thread = new Thread(StartThread);
            m_thread.Start();
        }

        public static void StartThread()
        {
            try
            {
                // Test movie scrapers
                TestScrapeMovieJavLibrary1();
                TestScrapeMovieJavLibrary2();
                TestScrapeMovieJavLibrary3();
                TestScrapeMovieJavDatabase1();
                TestScrapeMovieJavDatabase2();
                TestScrapeMovieJavSeenTv1();
                TestScrapeMovieJavSeenTv2();
                TestScrapeMovieJavSeenTv3();

                // Test actress scrapers
                TestScrapeActressJavDatabase1();
                TestScrapeActressJavDatabase2();
                TestScrapeActressJavModel1();
                TestScrapeActressJavModel2();
                TestScrapeActressJavBody1();
                TestScrapeActressJavBody2();
                TestScrapeActressAsianScreens1();
                TestScrapeActressAsianScreens2();
            }
            catch (Exception ex)
            {
                m_exception = ex;
            }

            // Invoke event on main thread when finished with tests
            if (m_dispatcher.HasShutdownStarted == false)
                m_dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate () { m_testsFinished?.Invoke(null, new EventArgs()); }));
        }

        private static void TestScrapeMovieJavLibrary1()
        {
            var metadata = new MovieMetadata("IESP-711");
            var module = new MovieJavLibrary(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();      
            CheckEqual("Minami Iroha Lesbian Ban - I Was Dragged Into A Lesbian Swamp By My Sister-In-Law -", module.Metadata.Title);
            CheckEqual("2022-12-22", module.Metadata.Premiered);
            CheckEqual(140, module.Metadata.Runtime);
            CheckEqual("Rin Kiku", module.Metadata.Director);
            CheckEqual("IE NERGY", module.Metadata.Studio);
            CheckEqual("Iesp", module.Metadata.Label);
            CheckEqual(4, module.Metadata.Genres.Count);
            CheckEqual("Lesbian", module.Metadata.Genres[0]);
            CheckEqual("Big Tits", module.Metadata.Genres[1]);
            CheckEqual("Slender", module.Metadata.Genres[2]);
            CheckEqual("Drama", module.Metadata.Genres[3]);
            CheckEqual(2, module.Metadata.Actors.Count);
            CheckEqual("Rika Aimi", module.Metadata.Actors[0].Name);
            CheckEqual("Iroha Minami", module.Metadata.Actors[1].Name);
            CheckNotEmpty(module.ImageSource);        
        }

        private static void TestScrapeMovieJavLibrary2()
        {
            var metadata = new MovieMetadata("BBAN-008");
            var module = new MovieJavLibrary(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual("Lesbian Journey Sakurai Ayu Yuna Shiina", module.Metadata.Title);
            CheckEqual("2014-07-07", module.Metadata.Premiered);
            CheckEqual(180, module.Metadata.Runtime);
            CheckEqual("", module.Metadata.Director);
            CheckEqual("Bibian", module.Metadata.Studio);
            CheckEqual("Bibian", module.Metadata.Label);
            CheckEqual(6, module.Metadata.Genres.Count);
            CheckEqual("Lesbian", module.Metadata.Genres[0]);
            CheckEqual("Outdoors", module.Metadata.Genres[1]);
            CheckEqual("Slender", module.Metadata.Genres[2]);
            CheckEqual("Documentary", module.Metadata.Genres[3]);
            CheckEqual("Lesbian Kiss", module.Metadata.Genres[4]);
            CheckEqual("Travel", module.Metadata.Genres[5]);
            CheckEqual(2, module.Metadata.Actors.Count);
            CheckEqual("Yuna Shiina", module.Metadata.Actors[0].Name);
            CheckEqual("Ayu Sakurai", module.Metadata.Actors[1].Name);
            CheckNotEmpty(module.ImageSource);
        }

        private static void TestScrapeMovieJavLibrary3()
        {
            var metadata = new MovieMetadata("FAKE-999");
            var module = new MovieJavLibrary(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        private static void TestScrapeMovieJavDatabase1()
        {
            var metadata = new MovieMetadata("IESP-711");
            var module = new MovieJavDatabase(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual("Minami Iroha Lesbian Ban - I Was Dragged Into A Lesbian Swamp By My Sister-In-Law -", module.Metadata.Title);
            CheckEqual("2022-12-22", module.Metadata.Premiered);
            CheckEqual(142, module.Metadata.Runtime);
            CheckEqual("chrysanthemum", module.Metadata.Director);
            CheckEqual("Ienergy", module.Metadata.Studio);
            CheckEqual(5, module.Metadata.Genres.Count);
            CheckEqual("Big Tits", module.Metadata.Genres[0]);
            CheckEqual("Drama", module.Metadata.Genres[1]);
            CheckEqual("Hi-Def", module.Metadata.Genres[2]);
            CheckEqual("Lesbian", module.Metadata.Genres[3]);
            CheckEqual("Slender", module.Metadata.Genres[4]);
            CheckEqual(2, module.Metadata.Actors.Count);
            CheckEqual("Mikana Mii", module.Metadata.Actors[0].Name);
            CheckEqual("Rika Aimi", module.Metadata.Actors[1].Name);
            CheckNotEmpty(module.ImageSource);
        }

        private static void TestScrapeMovieJavDatabase2()
        {
            var metadata = new MovieMetadata("FAKE-999");
            var module = new MovieJavDatabase(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        private static void TestScrapeMovieJavSeenTv1()
        {
            var metadata = new MovieMetadata("HAVD-959");
            var module = new MovieJavSeenTv(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual("- Kiss Kiss Of Ladies Lesbian Lesbian Lesbian Lesbian Secret Affair - Hibino", module.Metadata.Title);
            CheckEmpty(module.Metadata.Premiered);
            CheckEqual(0, module.Metadata.Runtime);
            CheckEqual("Dr.TORIHAMA", module.Metadata.Director);
            CheckEqual("Hibino", module.Metadata.Studio);
            CheckEmpty(module.Metadata.Label);
        }

        private static void TestScrapeMovieJavSeenTv2()
        {
            var metadata = new MovieMetadata("BBAN-333");
            var module = new MovieJavSeenTv(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual("Who Is She...? A Barely Legal Suddenly Targeted And Fucked - Aoi Nakajo, Mio Ichijo", module.Metadata.Title);
            CheckEqual("2021-7-2", module.Metadata.Premiered);
            CheckEqual(0, module.Metadata.Runtime);
            CheckEmpty(module.Metadata.Director);
            CheckEqual("bibian", module.Metadata.Studio);
            CheckEqual("bibian", module.Metadata.Label);
        }

        private static void TestScrapeMovieJavSeenTv3()
        {
            var metadata = new MovieMetadata("FAKE-999");
            var module = new MovieJavSeenTv(metadata, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        public static void TestScrapeActressJavDatabase1()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavDatabase(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            actressData = module.Actress;
            CheckEqual("Yui Hatano", actressData.Name);
            CheckEqual("波多野結衣", actressData.JapaneseName);
            CheckEqual(1988, actressData.DobYear);
            CheckEqual(5, actressData.DobMonth);
            CheckEqual(24, actressData.DobDay);
            CheckEqual(163, actressData.Height);
            CheckEqual("E", actressData.Cup);
            CheckEqual(86, actressData.Bust);
            CheckEqual(59, actressData.Waist);
            CheckEqual(94, actressData.Hips);
            CheckEqual("A", actressData.BloodType);
            CheckNotEmpty(module.ImageSource);
        }

        public static void TestScrapeActressJavDatabase2()
        {
            var actressData = new ActressData("Notta Realperson");
            var module = new ActressJavDatabase(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        public static void TestScrapeActressJavModel1()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavModel(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            actressData = module.Actress;
            CheckEqual("Yui Hatano", actressData.Name);
            CheckEqual("波多野結衣", actressData.JapaneseName);
            CheckEqual(1988, actressData.DobYear);
            CheckEqual(5, actressData.DobMonth);
            CheckEqual(24, actressData.DobDay);
            CheckEqual(163, actressData.Height);
            CheckEqual(88, actressData.Bust);
            CheckEqual(59, actressData.Waist);
            CheckEqual(85, actressData.Hips);
            CheckEqual("A", actressData.BloodType);
            CheckNotEmpty(module.ImageSource);
        }

        public static void TestScrapeActressJavModel2()
        {
            var actressData = new ActressData("Notta Realperson");
            var module = new ActressJavModel(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        public static void TestScrapeActressJavBody1()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressJavBody(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            actressData = module.Actress;
            CheckEqual("Yui Hatano", actressData.Name);
            CheckEqual("波多野結衣", actressData.JapaneseName);
            CheckEqual(1988, actressData.DobYear);
            CheckEqual(5, actressData.DobMonth);
            CheckEqual(24, actressData.DobDay);
            CheckEqual(163, actressData.Height);
            CheckEqual(88, actressData.Bust);
            CheckEqual(59, actressData.Waist);
            CheckEqual(85, actressData.Hips);
            CheckEqual("A", actressData.BloodType);
            CheckNotEmpty(module.ImageSource);
        }

        public static void TestScrapeActressJavBody2()
        {
            var actressData = new ActressData("Notta Realperson");
            var module = new ActressJavBody(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        public static void TestScrapeActressAsianScreens1()
        {
            var actressData = new ActressData("Yui Hatano");
            var module = new ActressAsianScreens(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            actressData = module.Actress;
            CheckEqual("Yui Hatano", actressData.Name);
            CheckEqual(1988, actressData.DobYear);
            CheckEqual(5, actressData.DobMonth);
            CheckEqual(24, actressData.DobDay);
            CheckEqual(163, actressData.Height);
            CheckEqual(88, actressData.Bust);
            CheckEqual(59, actressData.Waist);
            CheckEqual(85, actressData.Hips);
            CheckEqual("E", actressData.Cup);
            CheckNotEmpty(module.ImageSource);
        }

        public static void TestScrapeActressAsianScreens2()
        {
            var actressData = new ActressData("Notta Realperson");
            var module = new ActressAsianScreens(actressData.Name, m_dispatcher, m_webBrowser, LanguageType.English);
            module.Scrape();
            CheckEqual(true, module.SearchNotFound);
        }

        private static void CheckEqual(string expected, string actual)
        {
            if (expected != actual)
                throw new Exception("Web scraping error");
        }

        private static void CheckEqual(int expected, int actual)
        {
            if (expected != actual)
                throw new Exception("Web scraping error");
        }

        private static void CheckEqual(bool expected, bool actual)
        {
            if (expected != actual)
                throw new Exception("Web scraping error");
        }

        private static void CheckEmpty(string actual)
        {
            if (String.IsNullOrEmpty(actual) == false)
                throw new Exception("Web scraping error");
        }

        private static void CheckNotEmpty(string actual)
        {
            if (String.IsNullOrEmpty(actual) == true)
                throw new Exception("Web scraping error");
        }

        // Delegate to signal when commands are finished executing
        public delegate void FinishTestsEventHandler(object sender, EventArgs e);
        public static event FinishTestsEventHandler m_testsFinished;

        private static Thread m_thread;
        private static Dispatcher m_dispatcher;
        private static WebBrowser m_webBrowser;
        public static Exception m_exception = null;
    }
}
