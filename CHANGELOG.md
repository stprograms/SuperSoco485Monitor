# SuperSoco RS485 Monitor

This file holds the Changelog of the project and all relevant changes.

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
