# Changelog

All significant changes are listed in this file.

## [1.1.13] - 2022-12-10
- Optimized Ctrl-A (select all) command in browser views

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
