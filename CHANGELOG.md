# SuperSoco RS485 Monitor

This file holds the Changelog of the project and all relevant changes.
## 2.0.0
This release holds a lot of adaptions. Most of the communication was decoded by
@linshuweitw. The documentation was heavily adapted based on that. The application
itself has been updated to dotnet 8.0 and is now using the new information

## Modified
- Updated documentation [#10](https://github.com/stprograms/SuperSoco485Monitor/issues/10)
- Updated telegram parsing [#16](https://github.com/stprograms/SuperSoco485Monitor/issues/16)

## Added
- Introduced github actions [#12](https://github.com/stprograms/SuperSoco485Monitor/issues/12)
- Introduced software unit tests [#17](https://github.com/stprograms/SuperSoco485Monitor/issues/17)
- Introduced timestamp for telegrams [#22](https://github.com/stprograms/SuperSoco485Monitor/issues/22)
- Introduced storing of telegrams with timestamp [#23](https://github.com/stprograms/SuperSoco485Monitor/issues/23)
- Showing offset to previous telegram in time view [#24](https://github.com/stprograms/SuperSoco485Monitor/issues/24)
- Showing offset to previous telegram in grouped view [#25](https://github.com/stprograms/SuperSoco485Monitor/issues/25)

## 1.2.0
### Modified
- Added discharging cycles to BMS telegram
- Added information from @pervolianinen regarding different interfaces
- Added unit information for ECUStatus speed and current [#6](https://github.com/stprograms/SuperSoco485Monitor/issues/6)

## 1.1.1
### Fixed
- Wrong data interpretation in BMS telegram

### Added
- VBreaker field in BMS telegram

## 1.1.0
### Added
- Implemented a console printer for formatted output per telegram
- Implemented TelegramPlayer for replaying telegrams from files
- First base information for GSM status
- Implemented SerialSimulator to replay data on the serial port
- Introduced interface IUserVisualizeable and LogPrinter, so the output can be selected by the user

### Changed
- Default COM Port in settings to COM1

## 1.0.0
Initial release including support for live parsing and parsing of stored files.
