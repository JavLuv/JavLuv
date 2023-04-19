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

        public MovieMetadata ScrapeMovie(string movieID, ref string coverImagePath, LanguageType language)
        {
            Logger.WriteInfo("Attempting to scrape metadata for " + movieID);

            bool downloadCoverImage = String.IsNullOrEmpty(coverImagePath) ? false : true;
            if (downloadCoverImage)
                Logger.WriteInfo("Attempting to download cover image to " + coverImagePath);

            var mergedMetadata = new MovieMetadata(movieID);

            // We initially create two scrapers, since each site has
            // strengths and weaknesses, and so we prefer to take
            // specific information from each if possible.
            
            // Create first scraper module and execute
            var javLibraryMetadata = new MovieMetadata();
            javLibraryMetadata.UniqueID.Value = movieID;
            var javLibrary = new MovieJavLibrary(javLibraryMetadata, language);
            javLibrary.Scrape();
            javLibraryMetadata = javLibrary.Metadata;
            if (downloadCoverImage)
                DownloadImage(ref coverImagePath, javLibrary.ImageSource);

            // Create second scraper module and execute
            var javDatabaseMetadata = new MovieMetadata();
            javDatabaseMetadata.UniqueID.Value = movieID;
            var javDatabase = new MovieJavDatabase(javDatabaseMetadata, language);
            javDatabase.Scrape();
            javDatabaseMetadata = javDatabase.Metadata;
            if (downloadCoverImage)
                DownloadImage(ref coverImagePath, javDatabase.ImageSource);

            // Merge the two scrape results, combining them according to which
            // returns the best results from both.
            mergedMetadata = MergePrimary(javLibraryMetadata, javDatabaseMetadata);           
            
            // Scrape secondary sources if required
            ScrapeSecondaryMovie(new MovieJavRaveClub(new MovieMetadata(movieID), language), mergedMetadata, ref coverImagePath);
            ScrapeSecondaryMovie(new MovieJavGuru(new MovieMetadata(movieID), language), mergedMetadata, ref coverImagePath);
            ScrapeSecondaryMovie(new MovieJavLand(new MovieMetadata(movieID), language), mergedMetadata, ref coverImagePath);
            ScrapeSecondaryMovie(new MovieJavSeenTv(new MovieMetadata(movieID), language), mergedMetadata, ref coverImagePath);
            ScrapeSecondaryMovie(new MovieJavBus(new MovieMetadata(movieID), language), mergedMetadata, ref coverImagePath);

            // Is this minimally acceptable?
            if (IsMovieMetadataAcceptable(mergedMetadata) == false)
                return null;

            // Clean up names as best we can
            foreach (ActorData actor in mergedMetadata.Actors)
                MovieUtils.FilterActorName(actor);

            // If we're any language but Japanese, perform special Japanese-language scrape to get original title
            if (language != LanguageType.Japanese && String.IsNullOrEmpty(mergedMetadata.OriginalTitle))
                mergedMetadata.OriginalTitle = ScrapeOriginalTitle(movieID);

            // Return the best metadata we can
            Logger.WriteInfo("Metadata for " + mergedMetadata.UniqueID.Value + " successfully downloaded");
            return mergedMetadata;
        }

        public ActressData ScrapeActress(ActorData actor, LanguageType language)
        {
            // Prepare actress data with names and alternate names
            var actressData = new ActressData();
            actressData.Name = actor.Name;
            foreach (string altname in actor.Aliases)
                actressData.AltNames.Add(altname);

            // Scrape actress and merge any new data
            return ScrapeActress(actressData, language);
        }

        public ActressData ScrapeActress(ActressData actressData, LanguageType language)
        {
            Logger.WriteInfo("Attempting to scrape information for " + actressData.Name);

            // Check all possible sites for actress info
            ScrapeActress(new ActressJavDatabase(actressData.Name, language), actressData);
            ScrapeActress(new ActressJavModel(actressData.Name, language), actressData);
            ScrapeActress(new ActressJavBody(actressData.Name, language), actressData);
            ScrapeActress(new ActressJavRaveClub(actressData.Name, language), actressData);

            // Check to make sure there are no duplicates images, and the index is still in range
            if (actressData.ImageFileNames.Count > 0)
            {
                actressData.ImageFileNames = Utilities.DeleteDuplicateFiles(Utilities.GetActressImageFolder(), actressData.ImageFileNames);
                actressData.ImageIndex = Math.Min(actressData.ImageIndex, actressData.ImageFileNames.Count - 1);
            }

            // Log success or failure
            if (MovieUtils.IsActressWorthShowing(actressData) == false)
                Logger.WriteWarning("Unable to find online information for " + actressData.Name + " or aliases");
            else
                Logger.WriteInfo("Found information for " + actressData.Name);

            return actressData;
        }

        private ActressData ScrapeActress(ModuleActress module, ActressData actressData)
        {
            if (IsActressDataComplete(actressData))
                return actressData;
            module.Scrape();
            MergeActressData(actressData, module.Actress);
            ScrapeAltNamesIfNotAcceptable(actressData, module);
            DownloadActressImage(actressData, module);
            return actressData;
        }

        public string ScrapeOriginalTitle(string movieID)
        {
            var japaneseMetadata = ScrapeJapaneseMetadata(movieID, CompletionLevel.Minimal);
            if (japaneseMetadata == null)
            {
                Logger.WriteWarning(String.Format("Japanese title for movie {0} was not found.", movieID));
                return String.Empty;
            }
            return japaneseMetadata.Title;
        }

        public bool DownloadCoverImage(string movieID, ref string coverImagePath)
        {
            bool retVal = false;

            var metadata = new MovieMetadata();
            metadata.UniqueID.Value = movieID;

            // Language doesn't really matter, but English is the most commonly-supported language of
            // all the sites JavLuv scrapes.

            // Download images from all sites and determine the best quality
            var javLibrary = new MovieJavLibrary(metadata, LanguageType.English);
            javLibrary.Scrape();
            if (DownloadImage(ref coverImagePath, javLibrary.ImageSource))
                retVal = true;

            var javDatabase = new MovieJavDatabase(metadata, LanguageType.English);
            javDatabase.Scrape();
            if (DownloadImage(ref coverImagePath, javDatabase.ImageSource))
                retVal = true;

            var javGuru = new MovieJavGuru(metadata, LanguageType.English);
            javGuru.Scrape();
            if (DownloadImage(ref coverImagePath, javGuru.ImageSource))
                retVal = true;

            // Only continue past this point if we haven't yet downloaded a file
            if (retVal == false)
            {
                var javRaveClub = new MovieJavRaveClub(metadata, LanguageType.English);
                javRaveClub.Scrape();
                if (DownloadImage(ref coverImagePath, javRaveClub.ImageSource))
                    retVal = true;
            }

            if (retVal == false)
            {
                var javLand = new MovieJavLand(metadata, LanguageType.English);
                javLand.Scrape();
                if (DownloadImage(ref coverImagePath, javLand.ImageSource))
                    retVal = true;
            }

            if (retVal == false)
            {
                var javSeenTv = new MovieJavSeenTv(metadata, LanguageType.English);
                javSeenTv.Scrape();
                if (DownloadImage(ref coverImagePath, javSeenTv.ImageSource))
                    retVal = true;
            }

            if (retVal == false)
            {
                var javBus = new MovieJavBus(metadata, LanguageType.English);
                javBus.Scrape();
                if (DownloadImage(ref coverImagePath, javBus.ImageSource))
                    retVal = true;
            }

            return retVal;
        }

        #endregion

        #region Private Functions

        private void ScrapeSecondaryMovie(ModuleMovie moduleMovie, MovieMetadata mergedMetadata, ref string coverImagePath)
        {
            bool downloadCoverImage = String.IsNullOrEmpty(coverImagePath) ? false : true;
            if (IsMovieMetadataComplete(mergedMetadata) == true && downloadCoverImage == false)
                return;
            var scrapedMetadata = new MovieMetadata(mergedMetadata.UniqueID.Value);
            moduleMovie.Scrape();
            scrapedMetadata = moduleMovie.Metadata;
            if (downloadCoverImage)
                DownloadImage(ref coverImagePath, moduleMovie.ImageSource);
            mergedMetadata = MergeSecondary(mergedMetadata, scrapedMetadata);
        }

        private MovieMetadata ScrapeJapaneseMetadata(string movieID, CompletionLevel completion)
        {
            var combinedMetadata = new MovieMetadata();
            combinedMetadata.UniqueID.Value = movieID;
            string coverImagePath = String.Empty;
            ScrapeSecondaryMovie(new MovieJavDatabase(new MovieMetadata(movieID), LanguageType.Japanese), combinedMetadata, ref coverImagePath);
            if (IsMovieMetadataCompleteOrAcceptable(combinedMetadata, completion))
                return combinedMetadata;
            ScrapeSecondaryMovie(new MovieJavLibrary(new MovieMetadata(movieID), LanguageType.Japanese), combinedMetadata, ref coverImagePath);
            if (IsMovieMetadataCompleteOrAcceptable(combinedMetadata, completion))
                return combinedMetadata;
            ScrapeSecondaryMovie(new MovieJavLand(new MovieMetadata(movieID), LanguageType.Japanese), combinedMetadata, ref coverImagePath);
            if (IsMovieMetadataCompleteOrAcceptable(combinedMetadata, completion))
                return combinedMetadata;
            ScrapeSecondaryMovie(new MovieJavBus(new MovieMetadata(movieID), LanguageType.Japanese), combinedMetadata, ref coverImagePath);
            return combinedMetadata;
        }

        private void DownloadActressImage(ActressData actressData, ModuleActress module)
        {
            if (String.IsNullOrEmpty(module.ImageSource) == false)
            {
                // Create destination filename and path
                string actressImagefolder = Utilities.GetActressImageFolder();
                string actressImagePath = Path.Combine(actressImagefolder, Guid.NewGuid().ToString());

                // Download image
                if (DownloadImage(ref actressImagePath, module.ImageSource))
                {
                    // Add the downloaded image to image filenames
                    actressData.ImageFileNames.Add(Path.GetFileName(actressImagePath));
                }
            }
        }

        private bool DownloadImage(ref string imagePath, string imageSource)
        {
            // Don't continue if we don't have full information
            if (String.IsNullOrEmpty(imageSource) || String.IsNullOrEmpty(imagePath))
                return false;

            // Set the appropriate extension
            string imageFilename = Path.ChangeExtension(imagePath, Path.GetExtension(imageSource));

            bool retVal = false;

            // Download cover image
            using (var client = new WebClient())
            {
                try
                {
                    Logger.WriteInfo("Downloading image from " + imageSource);

                    // May be partial source
                    if (imageSource.StartsWith("http") == false)
                        imageSource = "http:" + imageSource;

                    if (File.Exists(imagePath))
                    {
                        // Load existing image
                        ImageSource currentImage = LoadImageFromFile(imagePath);

                        // Get temp filename
                        string tempFileName = Path.GetTempFileName();

                        // Download the image file and load it
                        client.DownloadFile(imageSource, tempFileName);
                        if (IsBannedFile(tempFileName) == false)
                        {
                            ImageSource newImage = LoadImageFromFile(tempFileName);                   
                            if (newImage.Width > currentImage.Width && newImage.Height > currentImage.Height)
                            {
                                Logger.WriteInfo("New image is bigger than old image... Deleting " + imagePath);
                                File.Delete(imagePath);
                                Logger.WriteInfo("Replacing with new image: " + imageFilename);
                                File.Copy(tempFileName, imageFilename, true);
                                imagePath = imageFilename;
                                retVal = true;
                            }
                        }
                        File.Delete(tempFileName);                  
                    }
                    else
                    {
                        client.DownloadFile(imageSource, imageFilename);

                        if (IsBannedFile(imageFilename) == false)
                        {
                            // Load image to check quality
                            ImageSource newImage = LoadImageFromFile(imageFilename);
                        
                            // Don't allow tiny thumbnail images - they're probably invalid anyhow
                            if (newImage.Width < 100 || newImage.Height < 100)
                            {
                                Logger.WriteInfo("Image is too small to use.");
                                File.Delete(imageFilename);
                            }
                            else
                            {
                                Logger.WriteInfo("Saved image: " + imageFilename);
                                imagePath = imageFilename;
                                retVal=true;
                            }                  
                        }
                        else
                            File.Delete(imageFilename);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError("Error downloading image", ex);
                }
            }
            return retVal;
        }

        private bool IsBannedFile(string filename)
        {
            string checksum = Utilities.GetSHA1Checksum(filename);
            // "Uknown actress" image from JavDatabase
            if (checksum == "69-BB-2B-57-50-7E-18-0F-91-DB-2A-03-06-79-39-AA-75-EB-05-F3")
                return true;
            // "Unknown actress" image from JavRave.club
            else if (checksum == "EA-C4-BE-81-0E-EB-0A-56-C4-91-AF-BA-3E-41-FA-F6-06-64-F6-F2")
                return true;
            return false;
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
            combined.OriginalTitle = MergeStrings(javLibrary.OriginalTitle, javDatabase.OriginalTitle);
            combined.Premiered = MergeStrings(javLibrary.Premiered, javDatabase.Premiered);
            combined.Year = MergeNumbers(javLibrary.Year, javDatabase.Year);
            combined.Studio = MergeStrings(javLibrary.Studio, javDatabase.Studio);
            combined.Label = MergeStrings(javLibrary.Label, javDatabase.Label);
            combined.Runtime = MergeNumbers(javLibrary.Runtime, javDatabase.Runtime);
            combined.Director = MergeStrings(javLibrary.Director, javDatabase.Director);
            combined.Series = MergeStrings(javLibrary.Series, javDatabase.Series);
            combined.Genres = MergeStringLists(javLibrary.Genres, javDatabase.Genres);

            // JavLibrary gives us alternate names, which is really handy, but since we're scraping
            // actress data from JavDatabase first, we'll initially get actresses from them and 
            // try merging in other actresses later.  
            combined.Actors = MergeActors(javDatabase.Actors, javLibrary.Actors);
            return combined;
        }

        private MovieMetadata MergeSecondary(MovieMetadata primary, MovieMetadata secondary)
        {
            primary.Title = MergeStrings(primary.Title, secondary.Title);
            primary.OriginalTitle = MergeStrings(primary.OriginalTitle, secondary.OriginalTitle);
            primary.Premiered = MergeStrings(primary.Premiered, secondary.Premiered);
            primary.Year = MergeNumbers(primary.Year, secondary.Year);
            primary.Studio = MergeStrings(primary.Studio, secondary.Studio);
            primary.Label = MergeStrings(primary.Label, secondary.Label);
            primary.Runtime = MergeNumbers(primary.Runtime, secondary.Runtime);
            primary.Director = MergeStrings(primary.Director, secondary.Director);
            primary.Series = MergeStrings(primary.Series, secondary.Series);
            primary.Genres = MergeStringLists(primary.Genres, secondary.Genres);

            // For secondary sources, don't merge actors.  Just copy actors if none already exist.
            if (primary.Actors.Count == 0 && secondary.Actors.Count != 0)
                primary.Actors = secondary.Actors;
            return primary;
        }

        private List<ActorData> MergeActors(List<ActorData> a, List<ActorData> b)
        {
            if (a.Count == 0)
                return b;
            foreach (var actorB in b)
            {
                bool mergedActor = false;
                foreach (var actorA in a)
                {
                    if (MovieUtils.AreActorsEquivalent(actorA, actorB))
                    {
                        MergeActors(actorA, actorB);
                        mergedActor = true;
                        break;
                    }
                }
                if (mergedActor == false)
                    a.Add(actorB);
            }
            return a;
        }

        private void MergeActors(ActorData a, ActorData b)
        {
            // Do nothing with main names equivalent - this is the norm
            if (a.Name != b.Name)
            {
                // If b's main name is different, we list it as an alias
                // if it's not already in a's aliases.
                bool foundInAliases = false;
                foreach (var name in a.Aliases)
                {
                    if (name == b.Name)
                    {
                        foundInAliases = true;
                        break;
                    }
                }
                if (foundInAliases == false)
                    a.Aliases.Add(b.Name);
            }

            // Merge b's aliases into a's alias list.
            // Check through all of b's aliases.
            foreach (var aliasB in b.Aliases)
            {
                // If the alias is equivalent to a's name, do nothing
                if (aliasB == a.Name)
                    continue;

                // Check to see if b's alias is already in a's list.  If not,
                // we'll add it as another alias.
                bool foundInAliases = false;
                foreach (var aliasA in a.Aliases)
                {
                    if (aliasA == aliasB)
                    {
                        foundInAliases = true;
                        break;
                    }
                }
                if (foundInAliases == false)
                    a.Aliases.Add(aliasB);
            }
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

        private bool IsMovieMetadataCompleteOrAcceptable(MovieMetadata metadata, CompletionLevel completion)
        {
            if (completion == CompletionLevel.Minimal)
                return IsMovieMetadataAcceptable(metadata);
            else
                return IsMovieMetadataComplete(metadata);
        }

        private bool IsMovieMetadataComplete(MovieMetadata metadata)
        {
            if (String.IsNullOrEmpty(metadata.Title))
                return false;
            if (String.IsNullOrEmpty(metadata.OriginalTitle))
                return false;
            if (String.IsNullOrEmpty(metadata.Premiered))
                return false;
            if (String.IsNullOrEmpty(metadata.Studio))
                return false;
            if (String.IsNullOrEmpty(metadata.Label))
                return false;
            if (String.IsNullOrEmpty(metadata.Director))
                return false;
            if (String.IsNullOrEmpty(metadata.Series))
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
            return true;
        }

        private void ScrapeAltNamesIfNotAcceptable(ActressData actressData, ModuleActress module)
        {
            // If failed to find or adequately populate data, try existing aliases
            if (IsActressDataAcceptable(actressData) == false)
            {
                foreach (string altName in actressData.AltNames)
                {
                    module.Name = altName;
                    module.Scrape();
                    MergeActressData(actressData, module.Actress);
                    if (IsActressDataAcceptable(actressData))
                        break;
                }          
            }
        }

        private ActressData NewActressFrom(ActressData actressData)
        {
            ActressData newActressData = new ActressData();
            newActressData.Name = actressData.Name;
            foreach (string altName in actressData.AltNames)
                newActressData.AltNames.Add(altName);
            return newActressData;
        }

        private void MergeActressData(ActressData a, ActressData b)
        {
            if (b == null)
                return;
            a.ImageFileNames.Concat(b.ImageFileNames);
            if (IsActressDataComplete(a))
                return;
            if (String.IsNullOrEmpty(a.JapaneseName))
                a.JapaneseName = b.JapaneseName;
            foreach (string altName in b.AltNames)
            {
                if (Utilities.Equals(altName, a.AltNames))
                    a.AltNames.Add(altName);
            }
            if (a.DobYear == 0)
                a.DobYear = b.DobYear;
            if (a.DobMonth == 0)
                a.DobMonth = b.DobMonth;
            if (a.DobDay == 0)
                a.DobDay = b.DobDay;
            if (a.Height < 50)
                a.Height = b.Height;
            if (String.IsNullOrEmpty(a.Cup))
                a.Cup = b.Cup;
            if (a.Bust == 0)
                a.Bust = b.Bust;
            if (a.Waist == 0)
                a.Waist = b.Waist;
            if (a.Hips == 0)
                a.Hips = b.Hips;
            if (String.IsNullOrEmpty(a.BloodType))
                a.BloodType = b.BloodType;
        }

        private bool IsActressDataComplete(ActressData actressData)
        {
            if (actressData == null)
                return false;
            if (String.IsNullOrEmpty(actressData.Name))
                return false;
            if (String.IsNullOrEmpty(actressData.JapaneseName))
                return false;
            if (actressData.DobYear == 0)
                return false;
            if (actressData.DobMonth == 0)
                return false;
            if (actressData.DobDay == 0)
                return false;
            if (actressData.Height == 0)
                return false;
            if (String.IsNullOrEmpty(actressData.Cup))
                return false;
            if (actressData.Bust == 0)
                return false;
            if (actressData.Waist == 0)
                return false;
            if (actressData.Hips == 0)
                return false;
            if (String.IsNullOrEmpty(actressData.BloodType))
                return false;
            return true;
        }

        private bool IsActressDataAcceptable(ActressData actressData)
        {
            if (actressData == null)
                return false;
            if (actressData == null)
                return false;
            if (String.IsNullOrEmpty(actressData.Name))
                return false;
            if (String.IsNullOrEmpty(actressData.JapaneseName))
                return false;
            if (actressData.DobYear == 0)
                return false;
            if (actressData.DobMonth == 0)
                return false;
            if (actressData.DobDay == 0)
                return false;
            return true;
        }

        #endregion

        #region Private Members

        enum CompletionLevel
        {
            Minimal,
            Complete
        }

        #endregion
    }
}
