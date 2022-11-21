# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added
* DebugUtility.DrawDebugCircle.
* MathUtility.GetPerpendicularVector.

### Fixed
* Fixed TagMaskEditor messing up values when editing multiple objects.

## [1.1.0] - 2022-11-13

### Changed
* Made GameTag.cs generation far move robust.
  * Handle when there are more than 64 tags.
  * Handle all invalid characters in variable names.
  * Handle variable names that are keywords.
  * Handle tag strings containing quotation marks.
* Removed the warning message when generating GameTag.cs as it's no longer relevant.

## [1.0.0] - 2022-10-08

### Added
- Initial release, all files and documentation added.
