# Changelog

## 1.0.0

Initial release of Better Build Settings.

### Added

- Named build profiles for reusable Unity build setups.
- Modular build pipeline: enable only the modules required for a profile.
- Configurable output root, build folder name, and executable name.
- Automatic folder creation before build.
- Naming variables for consistent build artifacts:
    - `%configName%`
    - `%productName%`
    - `%target%`
    - `%date%`
    - `%date:yyyyMMdd_HHmmss%`
    - `%time%`
- Extensible token registry for project-specific naming rules.
