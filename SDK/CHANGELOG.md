# [1.0.9](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.9...v1.0.8) (2024-06-24)

### Improvements
- perf: Performance enhancements (MET-1370)
  - reduce the Dictionary instances allocations when logging custom events by re-using the passed dictionary and avoid creating a new temporary dictionary for the common event attributes
  - use the epoch millis for of eventTime instead of allocating memory for a new iso 8601 string

# [1.0.8](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.8...v1.0.7) (2024-06-20)

### Improvements
- feat: Adds an SDK option to control the log level

# [1.0.7](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.7...v1.0.6) (2024-06-19)

### Bug Fixes
- fix: Decoding empty server response (MET-1358)

# [1.0.6](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.6...v1.0.5) (2024-06-18)

### Bug Fixes
- fix: Error handling during the server's response decoding
- fix: Uses a different user ID on every run to avoid influencing the ODS response from the previously submitted offer events
- fix: Corrects the format of the timezone (MET-1354)
- fix: Corrects the default value for the locale (MET-1354)
- fix: EventsLogger uses the Unity API outside the main thread (MET-1353)

### Improvements
- docs: Description for DeviceInfo and the new SdkConfig options

# [1.0.5](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.5...v1.0.4) (2024-05-15)

### Bug Fixes

* fix: Moves the warning message to Awake
* fix(ingestion): Adds a configuration option to control the network timeout for calls to the Metica backend
* fix(ingestion): Null properties are ignored

### Improvements
* feat(Display Log): Improvements to the display log functionality

# [1.0.4](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.4...v1.0.3) (2024-04-25)

### Bug Fixes

* fix(events logger): The events flush cadence was not taken by MeticaAPI.Config

# [1.0.3](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.3...v1.0.2) (2024-04-23)

### Improvements

* feat(api): Adds the type MeticaSdkConfig to collect all the sdk configuration parameters in a single place


# [1.0.2](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.2...v1.0.1) (2024-04-19)


### Bug Fixes

* build: Corrects the package layout ([99d6ca8d](https://github.com/meticalabs/metica-unity-sdk/commit/99d6ca8de7ca5aa7be8c37ba9f144edf85307e5b))


# [1.0.1](https://github.com/meticalabs/metica-unity-sdk/compare/v1.0.1...v1.0.0) (2024-04-17)


### Bug Fixes

* fix(events logger): The custom event attributes were disregarded ([95db575f](https://github.com/meticalabs/metica-unity-sdk/commit/95db575f79a8f04f22089ab36e1eeb736552c148))


### Improvements

* perf: Uses object pooling to reduce the dynamic allocations of Unity Component instances ([23c9d73c](https://github.com/meticalabs/metica-unity-sdk/commit/23c9d73c7dcc25904287daf5792e5ccb22036ceb))


# [1.0.0](https://github.com/meticalabs/metica-unity-sdk/releases/tag/v1.0.0) (2024-03-19)

Initial release
