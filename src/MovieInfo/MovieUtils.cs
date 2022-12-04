using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MovieInfo
{
    public static class MovieUtils
    {
        #region Public Functions

        public static MovieData MoveRenameMovieData(MovieData movieData, string library, string folder, string movie, string cover, string preview, string metadata)
        {
            if (String.IsNullOrEmpty(library))
                return movieData;

            // Create new movie data, copying existing fields as default
            MovieData newMovieData = new MovieData(movieData);

            // Create new paths and filenames based on rename / move strings and embedded tokens 
            // associated with specific types of metadata.
            string newFolder = MoveRenameGetFolder(movieData, folder);
            if (String.IsNullOrEmpty(newFolder) == false)
                newMovieData.Path = Path.Combine(library, newFolder);
            else
                newMovieData.Path = library;
            newMovieData.SharedPath = String.IsNullOrEmpty(newFolder) ? true : false;
            newMovieData.Folder = Path.GetFileName(newMovieData.Path);

            for (int i = 0; i < movieData.MovieFileNames.Count; ++i)
            {
                string newMovie = MoveRenameGetFilename(movieData, movie, movieData.MovieFileNames.Count == 1 ? -1 : i);
                if (String.IsNullOrEmpty(newMovie) == false)
                {
                    newMovieData.MovieFileNames[i] = Path.ChangeExtension(newMovie, Path.GetExtension(movieData.MovieFileNames[i]));

                    // Subtitles, if they exist, should match corresponding movie names
                    if (movieData.SubtitleFileNames.Count > i && String.IsNullOrEmpty(movieData.SubtitleFileNames[i]) == false)
                        newMovieData.SubtitleFileNames[i] = Path.ChangeExtension(newMovieData.MovieFileNames[i], Path.GetExtension(movieData.SubtitleFileNames[i]));
                }
            }
            string newCover = MoveRenameGetFilename(movieData, cover);
            if (String.IsNullOrEmpty(newCover) == false)
                newMovieData.CoverFileName = Path.ChangeExtension(newCover, Path.GetExtension(movieData.CoverFileName));
            for (int i = 0; i < movieData.ThumbnailsFileNames.Count; ++i)
            {
                string newPreview = MoveRenameGetFilename(movieData, preview, movieData.ThumbnailsFileNames.Count == 1 ? -1 : i);
                if (String.IsNullOrEmpty(newPreview) == false)
                    newMovieData.ThumbnailsFileNames[i] = Path.ChangeExtension(newPreview, Path.GetExtension(movieData.ThumbnailsFileNames[i]));
            }
            string newMetadata = MoveRenameGetFilename(movieData, metadata);
            if (String.IsNullOrEmpty(newMetadata) == false)
                newMovieData.MetadataFileName = Path.ChangeExtension(newMetadata, Path.GetExtension(movieData.MetadataFileName));

            // Peform actual file and folder move/rename operation based on movie data and current settings
            MoveRenameFoldersAndFiles(movieData, newMovieData);

            return newMovieData;
        }

        public static void RemoveEmptyLibraryFolder(string library, string folder)
        {
            // Stop removing folders when we hit the root library folder
            if (Path.Equals(library, folder))
                return;

            if (Directory.Exists(folder) == false)
            {
                // Move up the tree and recursively try again
                RemoveEmptyLibraryFolder(library, Path.GetDirectoryName(folder));
            }
            else
            {
                // Check to see if the directory is empty
                try
                {
                    if (Directory.EnumerateFileSystemEntries(folder).Any() == false)
                    {
                        try
                        {
                            // Delete the empty folder
                            Directory.Delete(folder);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteWarning("Issue deleting folder " + folder, ex);
                            return;
                        }
                        // Move up the tree and recursively try again
                        RemoveEmptyLibraryFolder(library, Path.GetDirectoryName(folder));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteWarning("Issue enumerating files " + folder, ex);
                }
            }
        }

        public static bool FilterMetadata(MovieMetadata metadata, List<FilterPair> studioFilter, List<FilterPair> labelFilter, List<FilterPair> directorFilter, List<FilterPair> genreFilter)
        {
            bool changed = false;
            metadata.Title = metadata.Title.Trim();
            if (metadata.Title.StartsWith(metadata.UniqueID.Value))
                metadata.Title = metadata.Title.Substring(metadata.UniqueID.Value.Length).Trim();
            metadata.Studio = FilterField(metadata.Studio, studioFilter, ref changed);
            metadata.Label = FilterField(metadata.Label, labelFilter, ref changed);
            string director = metadata.Director.Trim();
            if (director != metadata.Director)
            {
                changed = true;
                metadata.Director = director;
            }
            metadata.Director = FilterField(metadata.Director, directorFilter, ref changed);
            metadata.Genres = Utilities.FilterWordList(metadata.Genres, genreFilter, ref changed);
            return changed;
        }

        public static List<List<string>> SearchSplit(string stringToSplit)
        {
            // Initially parse into a single list
            var initialSplit = new List<string>();
            bool inQuote = false;
            StringBuilder currentToken = new StringBuilder();
            for (int index = 0; index < stringToSplit.Length; ++index)
            {
                char currentCharacter = stringToSplit[index];
                if (currentCharacter == '"')
                {
                    // When we see a ", we need to decide whether we are
                    // at the start or send of a quoted section...
                    inQuote = !inQuote;
                }
                else if (currentCharacter == ' ' && inQuote == false)
                {
                    // We've come to the end of a token, so we find the token,
                    // trim it and add it to the collection of results...
                    string result = currentToken.ToString().Trim();
                    if (result != "") 
                        initialSplit.Add(result);

                    // We start a new token...
                    currentToken = new StringBuilder();
                }
                else
                {
                    // We've got a 'normal' character, so we add it to
                    // the curent token...
                    currentToken.Append(currentCharacter);
                }
            }

            // We've come to the end of the string, so we add the last token...
            string lastResult = currentToken.ToString().Trim();
            if (lastResult != "")
                initialSplit.Add(lastResult);

            // Now break into multiple list separated by the 'or' keyword
            var results = new List<List<string>>();
            var currTerms = new List<string>();
            results.Add(currTerms);
            foreach (string term in initialSplit)
            {
                if (String.Compare(term, "or", true) == 0)
                {
                    currTerms = new List<string>();
                    results.Add(currTerms);
                }
                else
                {
                    currTerms.Add(term);
                }
            }

            return results;
        }


        public static int MovieTitleCompare(string leftTitle, string rightTitle)
        {
            return Utilities.TitleNormalize(leftTitle).CompareTo(Utilities.TitleNormalize(rightTitle));
        }

        public static int MovieIDCompare(UniqueID leftID, UniqueID rightID)
        {
            int retVal = MovieIDCompareAlpha(leftID.Value, rightID.Value);
            if (retVal != 0)
                return retVal;
            return MovieIDCompareNumeric(leftID.Value, rightID.Value);
        }

        public static int MovieActressCompare(List<ActorData> leftActors, List<ActorData> rightActors)
        {
            for (int i = 0; i < Math.Min(leftActors.Count, rightActors.Count); ++i)
            {
                int retVal = String.Compare(leftActors[i].Name, rightActors[i].Name);
                if (retVal != 0)
                    return retVal;
            }
            if (leftActors.Count > rightActors.Count)
                return -1;
            else if (leftActors.Count < rightActors.Count)
                return 1;
            return 0;
        }

         public static string GenresToString(MovieData movieData)
        {
            var str = new StringBuilder();
            foreach (var s in movieData.Metadata.Genres)
            {
                str.Append(s);
                if (movieData.Metadata.Genres.IndexOf(s) < movieData.Metadata.Genres.Count - 1)
                    str.Append(", ");
            }
            return str.ToString();
        }

        public static bool StringToGenres(MovieData movieData, string stringValue)
        {
            var genres = stringValue.Split(',');
            bool genresChanged = false;
            if (genres.Length != movieData.Metadata.Genres.Count)
                genresChanged = true;
            else
            {
                foreach (var g in genres)
                {
                    var genre = g.Trim();
                    if (movieData.Metadata.Genres.Contains(genre) == false)
                    {
                        genresChanged = true;
                        break;
                    }
                }
            }
            if (genresChanged)
            {
                movieData.Metadata.Genres.Clear();
                foreach (var g in genres)
                {
                    var genre = g.Trim();
                    movieData.Metadata.Genres.Add(genre);
                }
                movieData.MetadataChanged = true;
            }
            return genresChanged;
        }

        public static bool ActressHasName(ActressData actress, string name)
        {
            if (String.IsNullOrEmpty(actress.Name) || String.IsNullOrEmpty(name))
                return false;
            if (String.Compare(actress.Name, name, true) == 0)
                return true;
            if (String.IsNullOrEmpty(actress.JapaneseName) == false && String.Compare(actress.JapaneseName, name, true) == 0)
                return true;
            if (Utilities.Equals(name, actress.AltNames, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        public static bool ActressMatchesActor(ActressData actress, ActorData actor)
        {
            if (ActressHasName(actress, actor.Name))
                return true;
            foreach (string alias in actor.Aliases)
            {
                if (ActressHasName(actress, alias))
                    return true;
            }
            return false;
        }

        public static void MergeActresses(ActressData primary, ActressData secondary)
        {
            if (Utilities.Equals(secondary.Name, primary.AltNames, StringComparison.OrdinalIgnoreCase) == false)
                primary.AltNames.Add(secondary.Name);
            foreach (string altName2 in secondary.AltNames)
            {
                if (Utilities.Equals(altName2, primary.AltNames, StringComparison.OrdinalIgnoreCase) == false)
                    primary.AltNames.Add(altName2);
            }
            if (String.IsNullOrEmpty(primary.JapaneseName))
                primary.JapaneseName = secondary.JapaneseName;
            if (primary.DobYear == 0)
                primary.DobYear = secondary.DobYear;
            if (primary.DobMonth == 0)
                primary.DobMonth = secondary.DobMonth;
            if (primary.DobDay == 0)
                primary.DobDay = secondary.DobDay;
            if (primary.Height == 0)
                primary.Height = secondary.Height;
            if (String.IsNullOrEmpty(primary.Cup))
                primary.Cup = secondary.Cup;
            if (primary.Bust == 0)
                primary.Bust = secondary.Bust;
            if (primary.Waist == 0)
                primary.Waist = secondary.Waist;
            if (primary.Hips == 0)
                primary.Hips = secondary.Hips;
            if (String.IsNullOrEmpty(primary.BloodType))
                primary.BloodType = secondary.BloodType;
            if (primary.UserRating == 0)
                primary.UserRating = secondary.UserRating;
            if (String.IsNullOrEmpty(secondary.Notes) == false)
            {
                if (String.IsNullOrEmpty(primary.Notes))
                    primary.Notes = secondary.Notes;
                else
                    primary.Notes += "\n" + secondary.Notes;
            }
            if (secondary.ImageFileNames.Count != 0)
            {
                primary.ImageFileNames = primary.ImageFileNames.Concat(secondary.ImageFileNames).ToList();
                primary.ImageFileNames = Utilities.DeleteDuplicateFiles(Utilities.GetActressImageFolder(), primary.ImageFileNames);
                if (primary.ImageIndex >= primary.ImageFileNames.Count)
                    primary.ImageIndex = 0;
            }
        }

        public static bool IsActressWorthShowing(ActressData actress)
        {
            if (actress == null)
                return false;
            if (actress.ImageFileNames.Count == 0)
                return false;
            if (String.IsNullOrEmpty(actress.JapaneseName) == false)
                return true;
            if (actress.DobYear != 0 && actress.DobMonth != 0 && actress.DobDay != 0)
                return true;
            if (actress.Height != 0)
                return true;
            if (String.IsNullOrEmpty(actress.Cup) == false)
                return true;
            if (actress.Bust != 0 && actress.Waist != 0 && actress.Hips != 0)
                return true;
            if (String.IsNullOrEmpty(actress.BloodType) == false)
                return true;
            return false;
        }
        public static int GetAgeFromDateOfBirthAndDate(int dobYear, int dobMonth, int dobDay, int dateYear, int dateMonth, int dateDay)
        {
            if (dobYear == 0)
                throw new ArgumentException("Age can't be calculated without a valid year of bitth");

            // We'll allow a rough estimate, assuming Jan 1 b-day if none is available
            var dateOfBirth = new DateTime(dobYear, Math.Max(dobMonth, 1), Math.Max(dobDay, 1));

            // Calculate age - a little trickier than you'd expect.  
            // Still not 100% precise, but good enough in 99.999% of cases.
            DateTime zeroTime = new DateTime(1, 1, 1);
            DateTime a = dateOfBirth;
            DateTime b = new DateTime(dateYear, dateMonth, dateDay);
            TimeSpan span = b - a;
            // Because we start at year 1 for the Gregorian
            // calendar, we must subtract a year here.
            return (zeroTime + span).Year - 1;
        }

        public static int GetAgeFromDateOfBirth(int dobYear, int dobMonth, int dobDay)
        {
            DateTime now = DateTime.Now;
            return GetAgeFromDateOfBirthAndDate(dobYear, dobMonth, dobDay, now.Year, now.Month, now.Day);
        }

        public static string UserRatingToStars(int userRating)
        {
            if (userRating == 0)
                return "unrated";
            StringBuilder sb = new StringBuilder(10);
            while (userRating >= 2)
            {
                sb.Append("\u2605");
                userRating -= 2;
            }
            if (userRating != 0)
                sb.Append("½");
            return sb.ToString();
        }

        public static void FilterActorName(ActorData actor)
        {
            // Some actors are listed as "First Last (AltFirst AltLast).
            // This function will split these out into main and alt names

            if (actor == null || String.IsNullOrEmpty(actor.Name))
                return;

            // Try cplitting name on parens
            string[] actorNames = actor.Name.Split("()".ToCharArray());
            if (actorNames.Length == 1)
            {
                // If those don't exist, just trim and return the first string
                actor.Name = actorNames[0].Trim();
            }
            else
            {
                // Assign the trimmed first part
                actor.Name = actorNames[0].Trim();

                // If we have one or more names in parens, next try splitting on commas
                string[] moreActorNames = actorNames[1].Split(',');
                foreach (string name in moreActorNames)
                {
                    // Add each name to the alias list if it doesn't exist
                    string trimmedName = name.Trim();
                    bool foundAlias = false;
                    foreach (string alias in actor.Aliases)
                    {
                        if (alias == trimmedName)
                        {
                            foundAlias = true;
                            break;
                        }
                    }
                    if (foundAlias == false)
                        actor.Aliases.Add(trimmedName);
                }
            }

            // Make all title case
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            actor.Name = textInfo.ToTitleCase(actor.Name);
            for (int i = 0; i < actor.Aliases.Count; ++i)
                actor.Aliases[i] = textInfo.ToTitleCase(actor.Aliases[i]);

            // Remove duplicates
            var nameSet = new HashSet<string>();
            foreach (var alias in actor.Aliases)
            {
                if (alias != actor.Name)
                    nameSet.Add(alias.Trim());
            }
            actor.Aliases.Clear();
            foreach (var alias in nameSet)
                actor.Aliases.Add(alias);
        }

        public static bool AreActorsEquivalent(ActorData a, ActorData b)
        {
            if (a.Name == b.Name)
                return true;
            if (Utilities.Equals(b.Name, a.Aliases, StringComparison.OrdinalIgnoreCase))
                return true;
            if (Utilities.Equals(a.Name, b.Aliases, StringComparison.OrdinalIgnoreCase))
                return true;
            foreach (var name in a.Aliases)
            {
                if (Utilities.Equals(name, b.Aliases, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return AreActorsNearlyEquivalent(a, b);
        }

        public static string ActorsToString(List<ActorData> actors)
        {
            var str = new StringBuilder();
            foreach (var actor in actors)
            {
                str.Append(actor.Name.Trim());
                if (actors.IndexOf(actor) < actors.Count - 1)
                    str.Append(", ");
            }
            return str.ToString();
        }

        public static bool StringToActors(string stringValue, ref List<ActorData> actors)
        {
            // Split all strings by comma separator and insert into a set, trimming whitespace
            var stringNames = stringValue.Split(',');
            var nameSet = new HashSet<string>();
            foreach (var stringName in stringNames)
                nameSet.Add(stringName.Trim());

            // Check for no changes
            if (nameSet.Count == actors.Count)
            {
                bool noChanges = true;
                foreach (var actor in actors)
                {
                    if (nameSet.Contains(actor.Name) == false)
                    {
                        noChanges = false;
                        break;
                    }
                }
                if (noChanges)
                    return false;
            }

            // Remove names and re-add them if a match in the set is found
            var newActors = new List<ActorData>();

            // First look for substitutions
            foreach (var actor in actors)
            {
                if (nameSet.Contains(actor.Name))
                {
                    newActors.Add(actor);
                    nameSet.Remove(actor.Name);
                }
                else
                {
                    // Check for reversed name
                    if (nameSet.Contains(Utilities.ReverseNames(actor.Name)))
                    {
                        newActors.Add(actor);
                        nameSet.Remove(actor.Name);
                    }
                    else
                    {
                        // Next, let's see if the names are similar - in which case is probably an edit
                        bool isSimilar = false;
                        string similarName = String.Empty;
                        foreach (var checkedName in nameSet)
                        {
                            if (Utilities.GetSimilarity(checkedName, actor.Name) > s_nameSimilarityThreshold)
                            {
                                isSimilar = true;
                                similarName = checkedName;
                                break;
                            }
                            if (Utilities.GetSimilarity(checkedName, Utilities.ReverseNames(actor.Name)) > s_nameSimilarityThreshold)
                            {
                                isSimilar = true;
                                similarName = checkedName;
                                break;
                            }
                        }
                        if (isSimilar)
                        {
                            actor.Name = similarName;
                            newActors.Add(actor);
                            nameSet.Remove(similarName);
                        }
                    }
                }
            }

            // Add any leftover names
            foreach (var addedName in nameSet)
            {
                var addedActor = new ActorData();
                addedActor.Name = addedName;
                newActors.Add(addedActor);
            }

            actors = newActors;

            return true;
        }

        public static string GetMovieResolution(string filename)
        {
            string resolution = String.Empty;

            // Create arguments for asfbin
            var args = new StringBuilder(1024);
            args.Append("-i ");
            args.Append("\"");
            args.Append(filename);
            args.Append("\"");

            try
            {
                // Create process
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg.exe",
                        Arguments = args.ToString(),
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                })
                {
                    StringBuilder sb = new StringBuilder(5000);

                    // Start process and capture output by line
                    process.Start();
                    while (!process.StandardError.EndOfStream)
                        sb.Append(process.StandardError.ReadLine());

                    Regex regex = new Regex(@"\d{2,5}x\d{2,5}");
                    string s = sb.ToString();
                    var matches = regex.Matches(s);
                    if (matches.Count > 0)
                    {
                        resolution = matches[0].ToString();
                    }
                }
            }
            catch (Exception)
            {
            }

            return resolution;
        }

        #endregion

        #region Private Functions    

        private static string MoveRenameGetFilename(MovieData movieData, string filename, int sequenceIndex = -1)
        {
            if (String.IsNullOrEmpty(filename))
                return String.Empty;

            // Always ensure there is a valid sequence signature if there is a valid sequence index
            if (sequenceIndex != -1)
            {
                if (filename.Contains("SEQUENCE") == false)
                    filename += "{SEQUENCE \"-\" ALPHA}";
            }

            // Getting a filename is identical to getting a folder, with the exception that
            // directory delimiters are not allowed.
            string fn = MoveRenameGetFolder(movieData, filename, sequenceIndex);
            if (fn.Contains('\\') || fn.Contains('/'))
                throw new Exception("Filename fields cannot contain directory delimiters");

            return fn;
        }

        private static string MoveRenameGetFolder(MovieData movieData, string folder, int sequenceIndex = -1)
        {
            if (String.IsNullOrEmpty(folder))
                return String.Empty;

            // Search for tokens to substitute for movie-specific paths
            List<string> tokens = new List<string>();
            string[] tokenPairs = folder.Split('{');
            foreach (string tp in tokenPairs)
            {
                string[] tokenSplit = tp.Split('}');
                foreach (string ts in tokenSplit)
                {
                    tokens.Add(ts);
                }
            }

            StringBuilder sb = new StringBuilder(200);
            while (tokens.Count > 0)
            {
                sb.Append(tokens.First());
                tokens.RemoveAt(0);
                if (tokens.Count > 0)
                {
                    sb.Append(MoveRenameParseToken(movieData, sequenceIndex, tokens.First()));
                    tokens.RemoveAt(0);
                }
            }

            return sb.ToString();
        }

        private static string MoveRenameParseToken(MovieData movieData, int sequenceIndex, string token)
        {
            StringBuilder sb = new StringBuilder(200);
            string originalToken = token;

            token.TrimStart();
            if (token.StartsWith("DVD-ID"))
            {
                token = token.Substring(6);
                sb.Append(movieData.Metadata.UniqueID.Value);
            }
            else if (token.StartsWith("STUDIO"))
            {
                token = token.Substring(6);
                if (String.IsNullOrEmpty(movieData.Metadata.Studio))
                    sb.Append("(Unknown)");
                else
                    sb.Append(FilterFileName(movieData.Metadata.Studio));
            }
            else if (token.StartsWith("TITLE"))
            {
                token = token.Substring(5).Trim();
                if (String.IsNullOrEmpty(movieData.Metadata.Title))
                    sb.Append("(Unknown)");
                else
                    sb.Append(FilterFileName(movieData.Metadata.Title, Math.Min(200, Utilities.ParseInitialDigits(token))));
            }
            else if (token.StartsWith("YEAR"))
            {
                token = token.Substring(4);
                if (movieData.Metadata.Year == 0)
                    sb.Append("(Unknown)");
                else
                    sb.Append(FilterFileName(movieData.Metadata.Year.ToString()));
            }
            else if (token.StartsWith("ACTRESS"))
            {
                token = token.Substring(7).TrimStart();
                int num = Math.Min(4, Math.Max(1, Utilities.ParseInitialDigits(token)));
                List<string> names = new List<string>();
                foreach (var actor in movieData.Metadata.Actors)
                    names.Add(actor.Name);
                names.Sort();
                if (names.Count == 0)
                {
                    sb.Append("(unknown actresses)");
                }
                else if (names.Count > num)
                {
                    sb.Append("(");
                    sb.Append((++num).ToString());
                    sb.Append(" or more actresses)");
                }
                else
                {
                    for (int i = 0; i < names.Count; ++i)
                    {
                        sb.Append(names[i]);
                        if (i == names.Count - 2)
                            sb.Append(names.Count == 2 ? " & " : ", & ");
                        else if (i < names.Count - 2)
                            sb.Append(", ");
                    }
                }
            }
            else if (token.StartsWith("USER_RATING"))
            {
                token = token.Substring(11);
                token = token.TrimStart();

                List<string> tokens = new List<string>();
                string[] tokenPairs = token.Split('=', '"');
                bool numeric = true;
                foreach (string tp in tokenPairs)
                {
                    if (String.IsNullOrEmpty(tp))
                        continue;
                    if (numeric)
                        tokens.Add(tp.Trim());
                    else
                        tokens.Add(tp);
                    numeric = !numeric;
                }

                if (tokens.Count % 2 != 0)
                    throw new Exception("Error in USER_RATING terms");

                for (int i = 0; i < tokens.Count; i += 2)
                {
                    string[] numbers = tokens[i].Split('-');
                    int numMin = Utilities.ParseInitialDigits(numbers[0].Trim());
                    int numMax = (numbers.Length == 1) ? numMin : Utilities.ParseInitialDigits(numbers[1].Trim());
                    if (numMax < numMin)
                    {
                        int temp = numMin;
                        numMin = numMax;
                        numMax = temp;
                    }
                    string dirPart = tokens[i + 1];
                    if (movieData.Metadata.UserRating >= numMin && movieData.Metadata.UserRating <= numMax)
                    {
                        sb.Append(FilterFileName(dirPart));
                        break;
                    }
                }
            }
            else if (token.StartsWith("SEQUENCE") && sequenceIndex != -1)
            {
                token = token.Substring(8).Trim();
                string[] subtokens = token.Split('\"');               
                foreach (string subtoken in subtokens)
                {
                    if (String.IsNullOrEmpty(subtoken))
                        continue;
                    else if (subtoken.Contains("ALPHA_LOWER"))
                        sb.Append((char)((int)('a') + sequenceIndex));
                    else if (subtoken.Contains("ALPHA"))
                        sb.Append((char)((int)('A') + sequenceIndex));
                    else if (subtoken.Contains("NUMBER"))
                        sb.Append(sequenceIndex.ToString());
                    else
                        sb.Append(subtoken);
                }
            }

            return sb.ToString();
        }

        private static string FilterFileName(string s, int maxNumber = -1)
        {
            // According to what I know about UTF-16, searching for single characters
            // like this should still work even on surrogate pairs, since they're disjoint.
            // Thus, no account need be made for the variable width nature when searching
            // for single characters in the Basic Multilingual Plane (BPM).

            bool lengthBreak = false;
            StringBuilder sb = new StringBuilder(200);
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in s)
            {
                bool legal = true;
                char substitution = (char)0;
                foreach (char ic in invalidChars)
                {
                    if (c == ic)
                    {
                        // Where possible, substitute with lookalike characters,
                        // which are legal in filenames.  Sneaky, eh?
                        switch (c)
                        {
                            case '?':
                                substitution = '？';
                                break;
                            case ':':
                                substitution = '˸';
                                break;
                            case '"':
                                substitution = '＂';
                                break;
                            case '<':
                                substitution = '＜';
                                break;
                            case '>':
                                substitution = '＞';
                                break;
                            case '*':
                                substitution = '⁎';
                                break;
                            case '(':
                                substitution = '(';
                                break;
                            case '}':
                                substitution = ')';
                                break;
                            case '/':
                                substitution = '⁄';
                                break;
                            case '\\':
                                substitution = '⑊';
                                break;
                            case '|':
                                substitution = '❘';
                                break;
                            default:
                                legal = false;
                                break;
                        }
                    }
                }
                if (legal)
                {
                    if (substitution != (char)0)
                        sb.Append(substitution);
                    else
                        sb.Append(c);
                    if (maxNumber != -1)
                    {
                        if (sb.Length >= maxNumber)
                        {
                            lengthBreak = true;
                            break;
                        }
                    }
                }
            }

            string name = sb.ToString();

            // If we're trimming based on length, try to trim neatly at a space and append an elipse
            if (lengthBreak == true)
            {
                int index = name.LastIndexOf(' ');
                if (index != -1 && (name.Length - index) < (name.Length * .75))
                    name = name.Substring(0, index);
            }

            // Folder names can't end with a period.
            name = name.TrimEnd('.', ' ');

            // If the length was trimmed, add an ellipse
            if (lengthBreak == true)
                name += "…";

            return name;
        }

        private static string FilterField(string text, List<FilterPair> filters, ref bool changed)
        {
            foreach(var filter in filters)
            {
                if (String.Compare(text, filter.Original, true) == 0)
                {
                    // Check to see if we're only changing case.  If so, we should first do a case-sensitive compare
                    if (String.Compare(filter.Original, filter.Filtered, true) == 0)
                    {
                        if (String.Compare(filter.Original, filter.Filtered, true) == 0)
                        {
                            changed = true;
                            return filter.Filtered;
                        }
                    }
                    else
                    {
                        changed = true;
                        return filter.Filtered;
                    }
                }
            }
            return text;
        }

        private static bool AreActorsNearlyEquivalent(ActorData a, ActorData b)
        {
            const float SimilarityThreshold = 0.65f;
            if (Utilities.GetSimilarity(a.Name, b.Name) > SimilarityThreshold)
                return true;
            if (GetSimilarityMatches(b.Name, a.Aliases, SimilarityThreshold))
                return true;
            if (GetSimilarityMatches(a.Name, b.Aliases, SimilarityThreshold))
                return true;
            foreach (var name in a.Aliases)
            {
                if (GetSimilarityMatches(name, b.Aliases, SimilarityThreshold))
                    return true;
            }
            return false;
        }
        private static bool GetSimilarityMatches(string s, List<string> strings, float threshold)
        {
            foreach (var str in strings)
            {
                if (Utilities.GetSimilarity(s, str) > threshold)
                    return true;
            }
            return false;
        }

        private static void MoveRenameFoldersAndFiles(MovieData movieData, MovieData newMovieData)
        {
            string sourceFolder = movieData.Path;

            if (newMovieData.SharedPath == false)
            {
                if (movieData.SharedPath == false)
                {
                    // In case of a folder collision, the destination path may be adjusted
                    newMovieData.Path = Utilities.MoveFolder(movieData.Path, newMovieData.Path);

                    // Since we're moving all files in the folder, we'll change the source folder as well.
                    sourceFolder = newMovieData.Path;
                }
                else
                {
                    // In case of a folder collision, the new folder path may be adjusted
                    newMovieData.Path = Utilities.CreateFolder(newMovieData.Path);
                }
            }

            // Move/rename individual files as needed
            for (int i = 0; i < movieData.MovieFileNames.Count; ++i)
                Utilities.MoveFile(Path.Combine(sourceFolder, movieData.MovieFileNames[i]), Path.Combine(newMovieData.Path, newMovieData.MovieFileNames[i]));
            Utilities.MoveFile(Path.Combine(sourceFolder, movieData.CoverFileName), Path.Combine(newMovieData.Path, newMovieData.CoverFileName));
            for (int i = 0; i < movieData.ThumbnailsFileNames.Count; ++i)
                Utilities.MoveFile(Path.Combine(sourceFolder, movieData.ThumbnailsFileNames[i]), Path.Combine(newMovieData.Path, newMovieData.ThumbnailsFileNames[i]));
            Utilities.MoveFile(Path.Combine(sourceFolder, movieData.MetadataFileName), Path.Combine(newMovieData.Path, newMovieData.MetadataFileName));
            for (int i = 0; i < movieData.SubtitleFileNames.Count; ++i)
                Utilities.MoveFile(Path.Combine(sourceFolder, movieData.SubtitleFileNames[i]), Path.Combine(newMovieData.Path, newMovieData.SubtitleFileNames[i]));
        }

         private static int MovieIDCompareAlpha(string leftID, string rightID)
        {
            for (int i = 0; i < Math.Min(leftID.Length, rightID.Length); ++i)
            {
                if (Char.IsDigit(leftID[i]) && Char.IsDigit(rightID[i]))
                    return 0;
                int cmp = leftID[i].CompareTo(rightID[i]);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        }

        private static int MovieIDCompareNumeric(string leftID, string rightID)
        {
            string leftNum = GetMovieIDNumericPart(leftID);
            string rightNum = GetMovieIDNumericPart(rightID);
            return leftNum.CompareTo(rightNum);
        }

        private static string GetMovieIDNumericPart(string ID)
        {
            var parts = ID.Split('-');
            if (parts.Length == 2)
            {
                string p = parts[1];
                int len = p.Length;
                if (len == 2)
                    p = "000" + p;
                else if (len == 3)
                    p = "00" + p;
                else if (len == 4)
                    p = "0" + p;
                return p;
            }
            return ID;
        }

        #endregion

        #region Private Members

        private static readonly double s_nameSimilarityThreshold = 0.65;

        #endregion
    }
}
