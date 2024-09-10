# Changelog

All significant changes are listed in this file.

## [1.2.6] - 2024-9-10
- Import cover no longer deletes selected source file

## [1.2.5] - 2024-9-6
- Add support for DMM ID format (e.g. ABC00123)
- Add option to always display rating in browser view

## [1.2.4] - 2024-8-17
- Add actress scraper for AsianScreens.com
- Improve scraper speed

## [1.2.3] - 2024-8-8
- Add option to display Japanese name order
- Fix issues with move / rename error handling
- Move / rename has a starting numeric index of 1 instead of 0
- Rescan files no longer automatically calls move / rename after

## [1.2.2] - 2024-8-7
- Allow web scraper to retry failed attempts

## [1.2.1] - 2024-8-6
- Webscraper reliability, performance, and logging improvements
- Add check for locked movie files before moving or renaming
- Fix exception in Japanese language title scanner

## [1.2.0] - 2024-8-3
- Replaced internal web scraper technology for better robustness
- Fix issue with cover image replacement not updating cached image

## [1.1.35] - 2024-5-30
- Add new string to JavLibrary title filter

## [1.1.34] - 2024-3-22
- Fix string sanitizing test
- Fix scanner errors for system folders
- Fix JavDatabase movie scraper
- Fix JavModel actress scraper
- Removed JavRaveClub scrapers due to captcha protection

## [1.1.33] - 2024-3-9
- Remove now-unsupported JavBus scraper

## [1.1.32] - 2023-12-20
- Fix bug preventing displsy when invisible characters are copied into search bar
- Add mpeg as recognized movie extensions

## [1.1.31] - 2023-9-23
- Add support for {RELEASE_DATE} for move/rename functionality
- Added 'Add to collection' option in Scan dialog

## [1.1.30] - 2023-6-26
- Add reverse-filter entry to JavDatabase movie scraper
- Copied movie detail text lists N/A in place of blank fields

## [1.1.29] - 2023-6-2
- Fix rare crash when copying movie list or details to clipboard

## [1.1.28] - 2023-5-24
- Fix JavDatabase scrapers broken due to website changes

## [1.1.27] - 2023-5-6
- Add automated tests for movie scrapers for improved future reliability
- Fix several issues in actress and movie scrapers
- Add filter to correct bad data in Actress database

## [1.1.26] - 2023-4-27
- Fix multiple issues with actress scrapers
- Add automated tests for actress scrapers for improved future reliability

## [1.1.25] - 2023-4-23
- Add support for playing iso files as movies
- Add actress scraping support from JavBody.com
- Fix minor issue preventing updating of some actresses
- Add additional capabilities to ID parser

## [1.1.24] - 2023-4-3
- Upgrade ffmpeg from 4.4.1 to 6.0
- Fix issue saving metadata when stored on removable drives

## [1.1.23] - 2023-2-26
- Random movie list is now stable even when adding or removing movies
- Added Update Actress context menu item in movie detail view
- Fix issue in scanner if nfo (metadata) fails to load, and add logging
- Should now preserve unknown nfo (metadata) XML elements

## [1.1.22] - 2023-1-8
- Fix crash on invalid movie date conversion when adding new actress to movie
- Fix error when converting empty actress list in movie details
- Fix JavBus scraper to recognize not-found IDs

## [1.1.21] - 2023-1-7
- Add new filter for "unknown actress" image
- Random sort only changes on app start or when new movie sort type is selected

## [1.1.20] - 2023-1-4
- Fix crash bug attempting to auto-import an image as though a movie were available
- Removed duplicate error on scan for everything but movie files
- Added several new scrapers for movies and actresses

## [1.1.19] - 2023-1-3
- Fixed actress scraping on JavDatabase.com for site redesign
- Fix issue causing window width to occasionally be restored incorrectly
- Language dropdown now lists both English and Japanese text in both languages
- Reduced minimum interval for version checks

## [1.1.18] - 2022-12-29
- Add random movie sort
- Rework ID parser to reduce chances of false positives
- Version dialog can show multiple versions ahead of current

## [1.1.17] - 2022-12-27
- Confirm automatic movie import with dialog

## [1.1.16] - 2022-12-26
- Adjusted number of actress images shown in movie view to prevent overflow
- Movie detail info will now scroll if necessary
- Add context menu command to copy list of selected movies to clipboard
- Movie resolution is now stored in metadata
- Can sort movies by resolution
- Can import and replace movies during scanning if same or better resolution
- Triple-clicking in textboxes selects all text

## [1.1.15] - 2022-12-18
- Optionally allow running multiple instances of JavLuv using read-only mode
- Updated and localized movie details' copy to clipboard command

## [1.1.14] - 2022-12-10
- Optimized Ctrl-A (select all) command in browser views
- Fixed issue with actress navigation when actress age is displayed in movie detail view
- Fix crash with actress age calculation in movie detail due to incomplete premiere date

## [1.1.13] - 2022-12-09
- Optionally show actress' age at movie premier date
- Add checkbox to designate movies as hard-subbed
- Can now navigate through all actresses in a movie
- The 'Regenerate metadata' command no longer restores movie metadata from backup
- Japanese movie title is now searched
- Fix crash in Japanese language actress web scraper

## [1.1.12] - 2022-12-04
- Add context menu option to import movie subtitle
- Pressing Ctrl-Space will play a random movie from all files currently shown in the movie browser
- Actresses in movie detail auto collapse with large numbers to avoid visual overflow
- When scanning, JavLuv will no longer report manually moved movies as duplicates
- Added link to JavLuv Wiki
- Fix crash caused by missing movie when entering movie detail view
- Fix issue in movie concatenation function when path contains one or more single quotes
- Fix issue with actress' average movie rating updating after initial display

## [1.1.11] - 2022-11-29
- Add support for .ts movie extension
- Fix issue preventing actress images from merging in Merge Actresses dialog
- Fix issue when navigating actress movies

## [1.1.10] - 2022-11-25
- Added additional actress sorting criteria
- Fix issue with number of movies shown on actress page when returning from movie view

## [1.1.9] - 2022-11-23
- Fix issue with scraper downloading incorrect movie metadata in rare circumstances
- Fix issues when editing actresses in movie detail view
- Fix issue when parsing longer IDs

## [1.1.8] - 2022-11-22
- Added visual actress viewer to movie detail view
- Added support for multiple themes with light theme

## [1.1.7] - 2022-11-19
- Add 'Delete movie' command, visible when showing advanced options
- External movie metadata files are saved over time instead of all at once, improving performance of batch editing
- "Delete local cache" button has been removed, since deleting the cache may now cause data loss until movie metadata is finished saving
- Added "date added" field to metadata and an option to sort by most recently added movies
- Opening folder in movie detail view now highlights first movie
- Fix issue with sorting by date
- Fix issue with launching actress images clean up command
- Fix issue with corrupted actress measurement data when website info is unavailable
- Fix issue causing occasional leftover cover images

## [1.1.6] - 2022-11-12
- Add one-time automatic command to clean up actress images
- Add code to mitigate image duplication bug
- Block username from appearing in log
- Fix issue with error state in taskbar icon remaining active after report is closed
- Fix issue with actress renaming and merging

## [1.1.5] - 2022-11-1
- Add support for "series" movie metadata
- Fixed incorrect count type showing on status bar
- Fixed issues in ID detection algorithm

## [1.1.4] - 2022-10-26
- Search can now use 'or' or negate terms by prepending with '-'
- Disable update actress command when scanner is in use
- Fix exception when cancelling actress update scan
- Fix scanner recognition of longer movie IDs
- JavLuv is now fully documented at: https://github.com/JavLuv/JavLuv/wiki

## [1.1.3] - 2022-10-23
- Actresses in movie metadata are updated after turning auto actress synchronize on in settings
- Actress movie counts are refreshed after scanning movies
- Fix issue causing actress height to fail to parse on JavDatabase.com
- Hover mouse above actress height to show value in feet and inches
- Rearrange advanced options in movie browser context menu

## [1.1.2] - 2022-10-17
- Actress notes field is now searchable
- Fix actress movie miscount when listed as alias
- Fix issue causing certain movies to incorrectly display on actress detail view
- Normalize director name in JavLibrary scraper
- Actresses now sort by name if other criteria is equal

## [1.1.1] - 2022-10-15
- Eliminate scanner errors for extra files in existing folders
- Replaced folder browser with newer version
- Executable changed to 64-bit

## [1.1.0] - 2022-10-13
- Add Actress viewer
- Fixed rare instance of import movies being moved to incorrect destination
- Improved ID detection algorithm
- Remembers last folder by operation instead of using a global setting

## [1.0.2] - 2022-9-22
- Added buttons to reset or merge filters in settings
- Fixed styling of version check dialog
- Fixed mistaken disabling of once-per-version check

## [1.0.1] - 2022-9-19
- Can now automatically check for new releases
- Original Japanese title is now scraped for non-Japanese languages
- Added toggle in detail view for non-Japanese languages to show original Japanese title
- Scan status now shows progress of metadata loaded from disk
- Fixed a rescan issue causing duplicate file entries
- Fixed issue if file is open during auto renaming causing dialog to hang
- Fixed issue causing empty metadata to appear in collection if cancelled while scanning folders
- Build installer and zip are now named with version number

## [1.0.0] - 2022-9-17
- Initial version
