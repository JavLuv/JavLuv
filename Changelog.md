# Changelog

All significant changes are listed in this file.

## [1.1.6] - 2022-11-2

- Add command to clean up actress images
- Add code to mitigate image duplication bug
- Block username from appearing in log

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
