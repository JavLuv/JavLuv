using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebScraper
{
    public class Scraper
    {
        #region Public Functions

        public MovieMetadata Scrape(string movieID, ref string coverImagePath, LanguageType language)
        {
            Logger.WriteInfo("Attempting to scrape metadata for " + movieID);

            bool downloadCoverImage = String.IsNullOrEmpty(coverImagePath) ? false : true;
            if (downloadCoverImage)
                Logger.WriteInfo("Attempting to download cover image to " + coverImagePath);

            // We initially create two scrapers, since each site has
            // strengths and weaknesses, and so we prefer to take
            // specific information from each if possible.

            // Create first scraper module and execute
            var javLibraryMetadata = new MovieMetadata();
            javLibraryMetadata.UniqueID.Value = movieID;
            var javLibrary = new ModuleJavLibrary(javLibraryMetadata, language);
            javLibrary.Scrape();
            if (downloadCoverImage)
                DownloadCoverImage(ref coverImagePath, javLibrary.CoverImageSource);

            // Create second scraper module and execute
            var javDatabaseMetadata = new MovieMetadata();
            javDatabaseMetadata.UniqueID.Value = movieID;
            var javDatabase = new ModuleJavDatabase(javDatabaseMetadata, language);
            javDatabase.Scrape();
            if (downloadCoverImage)
                DownloadCoverImage(ref coverImagePath, javDatabase.CoverImageSource);

            // Merge the two scrape results, combining them according to which
            // returns the best results from both.
            var mergedMetadata = MergePrimary(javLibraryMetadata, javDatabaseMetadata);

            // Check to see if we have a complete set of metadata
            if (IsMovieMetadataComplete(mergedMetadata) == false || downloadCoverImage)
            {
                // Try a secondary merge
                mergedMetadata = MergeSecondary(mergedMetadata, javLibraryMetadata);

                // If not complete, try additional JavLand scraper
                if (IsMovieMetadataComplete(mergedMetadata) == false || downloadCoverImage)
                {
                    var javLandMetadata = new MovieMetadata();
                    javLandMetadata.UniqueID.Value = movieID;
                    var javLand = new ModuleJavLand(javLandMetadata, language);
                    javLand.Scrape();
                    if (downloadCoverImage)
                        DownloadCoverImage(ref coverImagePath, javLand.CoverImageSource);
                    mergedMetadata = MergeSecondary(mergedMetadata, javLandMetadata);

                    // Try JavBus scraper
                    if (IsMovieMetadataComplete(mergedMetadata) == false || downloadCoverImage)
                    {
                        var javBusMetadata = new MovieMetadata();
                        javBusMetadata.UniqueID.Value = movieID;
                        var javBus = new ModuleJavBus(javBusMetadata, language);
                        javBus.Scrape();
                        if (downloadCoverImage)
                            DownloadCoverImage(ref coverImagePath, javBus.CoverImageSource);
                        mergedMetadata = MergeSecondary(mergedMetadata, javBusMetadata);
                    }
                }

                // Is this minimally acceptable?
                if (IsMovieMetadataAcceptable(mergedMetadata) == false)
                    return null;
            }

            // If we're any language but Japanese, perform special Japanese-language scrape to get original title
            if (language != LanguageType.Japanese)
                mergedMetadata.OriginalTitle = ScrapeOriginalTitle(movieID);

            // Return the best metadata we can
            Logger.WriteInfo("Metadata for " + mergedMetadata.UniqueID.Value + " successfully downloaded");
            return mergedMetadata;
        }

        public string ScrapeOriginalTitle(string movieID)
        {
            var titleMetadata = new MovieMetadata();
            titleMetadata.UniqueID.Value = movieID;
            var javDatabase = new ModuleJavDatabase(titleMetadata, LanguageType.Japanese);
            javDatabase.Scrape();
            if (String.IsNullOrEmpty(titleMetadata.Title))
            {
                var javLibrary = new ModuleJavLibrary(titleMetadata, LanguageType.Japanese);
                javLibrary.Scrape();
                if (String.IsNullOrEmpty(titleMetadata.Title))
                {
                    var javLand = new ModuleJavLand(titleMetadata, LanguageType.Japanese);
                    javLand.Scrape();
                    if (String.IsNullOrEmpty(titleMetadata.Title))
                    {
                        var javBus = new ModuleJavBus(titleMetadata, LanguageType.Japanese);
                        javBus.Scrape();
                    }
                }
            }

            if (String.IsNullOrEmpty(titleMetadata.Title))
                Logger.WriteWarning(String.Format("Japanese title for movie {0} was not found.", movieID));

            return titleMetadata.Title;
        }

        public bool DownloadCoverImage(string movieID, ref string coverImagePath)
        {
            bool retVal = false;

            var metadata = new MovieMetadata();
            metadata.UniqueID.Value = movieID;

            // Language doesn't really matter, but English is the most commonly-supported language of
            // all the sites JavLuv scrapes.

            // Download images from all sites and determine the best quality
            var javLibrary = new ModuleJavLibrary(metadata, LanguageType.English);
            javLibrary.Scrape();
            if (DownloadCoverImage(ref coverImagePath, javLibrary.CoverImageSource))
                retVal = true;

            var javDatabase = new ModuleJavDatabase(metadata, LanguageType.English);
            javDatabase.Scrape();
            if (DownloadCoverImage(ref coverImagePath, javLibrary.CoverImageSource))
                retVal = true;

            var javLand = new ModuleJavLand(metadata, LanguageType.English);
            javLand.Scrape();
            if (DownloadCoverImage(ref coverImagePath, javLibrary.CoverImageSource))
                retVal = true;

            var javBus = new ModuleJavBus(metadata, LanguageType.English);
            javBus.Scrape();
            if (DownloadCoverImage(ref coverImagePath, javLibrary.CoverImageSource))
                retVal = true;

            return retVal;
        }

        #endregion

        #region Private Functions

        private bool DownloadCoverImage(ref string coverImagePath, string coverImageSource)
        {
            // Don't continue if we don't have full information
            if (String.IsNullOrEmpty(coverImageSource) || String.IsNullOrEmpty(coverImagePath))
                return false;

            // Set the appropriate extension
            string coverImageFilename = Path.ChangeExtension(coverImagePath, Path.GetExtension(coverImageSource));

            bool retVal = false;

            // Download cover image
            using (var client = new WebClient())
            {
                try
                {
                    Logger.WriteInfo("Downloading cover art from " + coverImageSource);

                    // May be partial source
                    if (coverImageSource.StartsWith("http") == false)
                        coverImageSource = "http:" + coverImageSource;

                    if (File.Exists(coverImagePath))
                    {
                        // Load existing image
                        ImageSource currentImage = LoadImageFromFile(coverImagePath);

                        // Get temp filename
                        string tempFileName = Path.GetTempFileName();

                        // Download the image file and load it
                        client.DownloadFile(coverImageSource, tempFileName);

                        ImageSource newImage = LoadImageFromFile(tempFileName);                   
                        if (newImage.Width > currentImage.Width && newImage.Height > currentImage.Height)
                        {
                            Logger.WriteInfo("Replacing cover art: " + coverImageFilename);
                            File.Copy(tempFileName, coverImageFilename, true);
                            coverImagePath = coverImageFilename;
                            retVal = true;
                        }
                        File.Delete(tempFileName);                  
                    }
                    else
                    {
                        client.DownloadFile(coverImageSource, coverImageFilename);

                        // Load image to check quality
                        ImageSource newImage = LoadImageFromFile(coverImageFilename);
                        
                        // Don't allow tiny thumbnail images - they're probably invalid anyhow
                        if (newImage.Width < 100 || newImage.Height < 100)
                        {
                            Logger.WriteInfo("Cover art is too small to use.");
                            File.Delete(coverImageFilename);
                        }
                        else
                        {
                            Logger.WriteInfo("Saved cover art: " + coverImageFilename);
                            coverImagePath = coverImageFilename;
                            retVal=true;
                        }                  
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError("Error downloading cover image", ex);
                }
            }
            return retVal;
        }

        private ImageSource LoadImageFromFile(string filename)
        {
            using (Stream imageStreamSource = File.OpenRead(filename))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = imageStreamSource;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private MovieMetadata MergePrimary(MovieMetadata javLibrary, MovieMetadata javDatabase)
        {
            var combined = new MovieMetadata();
            combined.UniqueID = javDatabase.UniqueID;
            // Prefer JavDatabase for titles
            combined.Title = MergeStrings(javDatabase.Title, javLibrary.Title);
            // For all other info, prefer JavLibrary
            combined.Premiered = MergeStrings(javLibrary.Premiered, javDatabase.Premiered);
            combined.Year = MergeNumbers(javLibrary.Year, javDatabase.Year);
            combined.Studio = MergeStrings(javLibrary.Studio, javDatabase.Studio);
            combined.Label = MergeStrings(javLibrary.Label, javDatabase.Label);
            combined.Runtime = MergeNumbers(javLibrary.Runtime, javDatabase.Runtime);
            combined.Director = MergeStrings(javLibrary.Director, javDatabase.Director);
            combined.Series = MergeStrings(javLibrary.Series, javDatabase.Series);
            combined.Genres = MergeStringLists(javLibrary.Genres, javDatabase.Genres);
            // JAV Library seems more reliable for actors.  so don't use other sources
            // unless there's no choice.
            combined.Actors = (javLibrary.Actors.Count == 0) ? javDatabase.Actors : javLibrary.Actors;
            return combined;
        }

        private MovieMetadata MergeSecondary(MovieMetadata primary, MovieMetadata secondary)
        {
            primary.Title = MergeStrings(primary.Title, secondary.Title);
            primary.Premiered = MergeStrings(primary.Premiered, secondary.Premiered);
            primary.Year = MergeNumbers(primary.Year, secondary.Year);
            primary.Studio = MergeStrings(primary.Studio, secondary.Studio);
            primary.Label = MergeStrings(primary.Label, secondary.Label);
            primary.Runtime = MergeNumbers(primary.Runtime, secondary.Runtime);
            primary.Director = MergeStrings(primary.Director, secondary.Director);
            primary.Series = MergeStrings(primary.Series, secondary.Series);
            primary.Genres = MergeStringLists(primary.Genres, secondary.Genres);
            // Only use secondary actors if required.  Too easy to get alternate, incorrect spellings
            if (primary.Actors.Count == 0)
                primary.Actors = secondary.Actors;
            return primary;
        }

        private string MergeStrings(string a, string b)
        {
            if (String.IsNullOrEmpty(a))
                return b;
            return a;
        }

        private int MergeNumbers(int a, int b)
        {
            return (a == 0) ? b : a;
        }

        private List<string> MergeStringLists(List<string> a, List<string> b)
        {
            return a.Union(b, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private bool IsMovieMetadataComplete(MovieMetadata metadata)
        {
            if (String.IsNullOrEmpty(metadata.Title))
                return false;
            if (String.IsNullOrEmpty(metadata.Premiered))
                return false;
            if (String.IsNullOrEmpty(metadata.Studio))
                return false;
            if (metadata.Year == 0)
                return false;
            if (metadata.Runtime == 0)
                return false;
            if (metadata.Genres.Count == 0)
                return false;
            if (metadata.Actors.Count == 0)
                return false;
            return true;
        }

        private bool IsMovieMetadataAcceptable(MovieMetadata metadata)
        {
            if (String.IsNullOrEmpty(metadata.Title))
                return false;
            if (String.IsNullOrEmpty(metadata.Studio))
                return false;
            return true;
        }

        #endregion
    }
}
