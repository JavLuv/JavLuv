# JavLuv
JavLuv is a tool for organizing your Japanese Adult Video collection.  

# Requirements
JavLuv runs on Microsoft Windows 10 or later.  Visual Studio 2022 is required to compile the editor from source.  The Wix plugin is required to compile the installer.

# Features
* Automatic identification of movies by ID embedded in file or folder names
* Generation of Kodi-compatible .nfo metadata
* Blocking or renaming genre keywords
* Visual display of movies in thumbnail or in details
* Filter instantly by any keywords, such as ID, title, actrees, genres, folder names, etc
* Sort by title, ID, folder name, date, and rating
* Supports user ratings to identify favorites
* Automatic moving / renaming of movie folders and filenames according to configurable rules

JavLuv identifies movies by a alpha-numeric code (e.g. \[ABC-123\]) to identify a movie, and then retrieves information from a number of websites to generate metadata for the identified movie.

Metadata is stored in a Kodi-compatible XML file typically named the same as the movie file, but with a .nfo extension.  A local cache of this data is used, but the .nfo metadata is considered to be the authoritative source for the movies.

# Moving / Renaming
JavLuv has an automatic renaming feature.  It can move and rename folders and files according to a set of rules.  Metadata can be used to generate paths for filenames.  They are surrounded by curly brackets and a keyword, sometimes with additional parameters.  

## Metadata identifiers:
* {DVD-ID} - The unique ID of the movie, often in a form ABC-123 or similar.
* {TITLE #} - Title of the movie, followed by a number that indicates how many characters are allowed.  Folder concatenation attempts to break on the nearest word, and indicates this with an elipse.
* {ACTRESS #} - Name of actress(es) in title, with optional maximum number of actresses to list (default is 1).
* {STUDIO} - Name of the studio
* {YEAR} - Year movie was released
* {USER_RATING #-#="Folder1" #-#="Folder2"} - Allows substitution of multiple folder names according to the user rating (a value between 0 and 10).
* {SEQUENCE "-" ALPHA/ALPHA_LOWER/NUMBER} - Creates a consacutive set of trailing identifiers depending on the last identifier.  ALPHA creates uppercase letters (A, B, C), ALPHA_LOWER creates lowercase letters (a, b, c), and NUMBER creates a sequence of numbers (1, 2, 3).

Metadata identifiers and other path for filename data can be used in various fields used for different purposes.  

## Fields:
* Library Folder - Identifies the folder containing your JAV library.  Metadata cannot be used in this field.
* Folder - Defines the folder used to contain all files associated with a single movie.  If this field is blank, all files are stored in a single folder.
* Movie - Defines the filename used for movies.  Because multiple filenames of this type may be present, this field must end with a SEQUENCE.
* Cover - Defines the filename used for cover imagas.
* Preview - Defines the filename used for preview (thumbnail) images.  Because multiple filenames of this type may be present, this field must end with a SEQUENCE.
* Metadata - Defines the filename used for metadata (generated .nfo file).

The Folder field can define any terms that expand to a legal path.  For instance, it can be as simple as the name of a folder, although that wouldn't be all that useful.  It becomes more useful when combined with unique metadata.  For example, the following folder field:

    {USER_RATING 9-10="Favorites" 1-8="Library" 0="New"}\{ACTRESS}\{STUDIO}\{YEAR}\[{DVD-ID}] {TITLE 80}

will, given a movie with a user rating of 9, starring Sakura Sakuraba, a studio name of Example Studio, a release year of 2020, a DVD-ID of ABC-123, and a title of "Example Movie" will be moved or renamed as follows:

    Favorites\Sakura Sakuraba\Example Studio\2020\[ABC-123] Example Movie\

## Performing Move / Rename:
There are two ways to move / rename your movies.  If you check the Move / rename after scan option in the Scan Movies dialog, then this will be performed every time you scan a new movie.  It will automatically be moved into your library folder and named according to your preferred rules.

Alternately, you can select any number of movies in the browser view (CTRL-A selects all files), right click, and select Move / Rename... from the context menu.  It does no harm to attempt to move / rename files that have already had this operation performed on them.  If nothing needs moving or renaming, nothing will occur.