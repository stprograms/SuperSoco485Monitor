[![.NET Workflow for test building](https://github.com/stprograms/SuperSoco485Monitor/actions/workflows/build.yml/badge.svg)](https://github.com/stprograms/SuperSoco485Monitor/actions/workflows/build.yml)

# SuperSoco485Monitor
C# Application to monitor the communication on the internal RS485 Bus on Super Soco Motorcycles

This small application is used for debug and analysis of the data received from the RS485 bus of Super Soco Motorcycles. It is not compatible with newer motorcycles that use CAN bus as the communication medium. Since this application is a helper for first analysis, the code is more or less written quick and dirty.

Information about the Baudrate, Structure of the telegrams and content of the telegrams has been derived from the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) of [Xmanu12](https://github.com/Xmanu12).

## Configuration
This section describes how the behavior of the application can be configured.

### appsettings.json
The output of the application is done using the NLog library. That means that the output can be configured using the NLog object in the appsettings.json. It is recommended though that these settings are not changed.

Other than NLog, the application can be configured using the following options in the `Monitor` object in `appsettings.json`.
```json
{
"Monitor": {
    "port": "COM6",
    "writeRawData" : true,
    "outputDir" : "",
    "replayCycle" : 5
}
}
```

| Setting      | Type    | Description                                                                                                                  |
| ------------ | ------- | ---------------------------------------------------------------------------------------------------------------------------- |
| port         | string  | Name of the port to open. eg. COM6                                                                                           |
| writeRawData | boolean | Write all received data also to a binary file. A new file (with timestamp) is created every time the application is started. |
| outputDir    | string  | (optional) If configured, create the binary files in the given folder.                                                       |
| replayCycle  | double  | Cycle for replaying telegrams in milliseconds                                                                                |


### Console arguments
If the application is called without arguments, the application will try to connect to the configured com port and parse the telegrams live. Each telegram is then exported to the console using nlog. The format is either binary or if the telegram is known, written in a decoded way.

If a the path of a binary file is provided as a console argument, the content of the file will be parsed and printed to the console, instead of using the configured serial port.

#### Using the Monitor as simulator
To replay telegrams previously recorded, use a binary file as command argument and provide the flag `-r`. This will open the serial port set in the configuration file and replay the telegrams from the binary file. The timing will not be the same as the original, but the telegrams will be sent in a configurable cycle.


## Necessary hardware
For connecting to the internal RS485 bus of the motorcycle, you need a RS485 to serial converter. You can use all possible versions, like
- RS485 to RS232 com port
- RS485 to USB
- RS485 to bluetooth like mentioned in the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) project under documents.

The best way to connect the RS485 is the 4 pin JST SM connector under the seat that connects the internals and the external battery plug. There, the two middle connectors are the RS485 interface.

### JST Connector pinning
Super Soco (at least TC Max), uses a JST SM 4 pin connector under the seat. This connector can be disconnected if the motorcycle is not charged through the external charger port. Instead of the connection to the external charger, a JST SM 4 pin plug can be connected and attached to a RS485 converter. The pinning of the connector is as following:

| Pin |    Signal    |
| --- | :----------: |
| 1   | brake signal |
| 2   |   RX- / B    |
| 3   |   RX+ / A    |
| 4   |    Ground    |

The brake signal is used to prevent movement when the charger is plugged in.

# RS485 Protocol
The following information is taken from the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) project and is sumarized here. All the information has been reverse engineered and can therefor hold errors and unknown data.

The communication is using **9600 Baud**, with 8 bit data and 1 stop bit. It is based on Read requests and Read responses and data is transmitted in telegrams.
## Generic information and structure

Each telegram starts with two bytes specifying a request or a response, followed by one byte source (id) and one byte destination (id). After that, the length of the user data in Bytes and the data itself is transmitted. Lastly, a one byte checksum and the end tag 0x0D terminates the telegram.

| Byte |   0   |   1   |   2    |   3   |    4    |  4 + 1  |  4 + 2  |  ...  |   4 + n   | 4 + n + 1 | 4 + n + 2 |
| :--- | :---: | :---: | :----: | :---: | :-----: | :-----: | :-----: | :---: | :-------: | :-------: | :-------: |
|      | type1 | type2 | source | dest  | len (n) | data[0] | data[1] |  ...  | data[n-1] | checksum  |   0x0D    |

There are two known combinations for the type:
| Byte1 | Byte2 | Type          |
| ----: | :---- | ------------- |
|  0xC5 | 0x5C  | Read request  |
|  0xB6 | 0x6B  | Read response |

## Checksum calculation
The checksum byte is calculated by making an XOR calculation of the databytes and the length byte. The following C# example shall deepen the understanding how the checksum is calculated.

```c#
// dataLen holds the value of the length byte
// rawData is a byte[] holding the raw data between the length byte and the checksum byte

byte calcCheck = dataLen;
foreach (byte b in rawData)
{
    calcCheck ^= b;
}

if (calcCheck == checksum)
{
    // Data is valid
}

```

## Decoded telegrams
The following telegrams and packages of read responses are already decoded.

### BMS Status (Read Response 0xAA5A)

| Byte (len=10) |    0    |   1   |   2   |   3    |   4    |   5    |     6     |     7     |    8     |    9     |
| ------------- | :-----: | :---: | :---: | :----: | :----: | :----: | :-------: | :-------: | :------: | :------: |
|               | Voltage |  SoC  | Temp  | Charge | CycleH | CycleL | DisCycleH | DisCycleL | VBreaker | Charging |

#### Description of the variables
| Variable      | Description                                  | Unit                                                                                              | Data Type     |
| ------------- | -------------------------------------------- | ------------------------------------------------------------------------------------------------- | ------------- |
| Voltage       | current Voltage of the Battery in V          | Volts[V]                                                                                          | unsigned byte |
| SoC           | State of Charge in %                         | Percent [%]                                                                                       | unsigned byte |
| Temp          | current temperatur of the BMS                | Degree C [°C]                                                                                     | signed byte   |
| Charge        | current charging or discharging current in A | Ampere [A]                                                                                        | signed byte   |
| Cycle[H/L]    | Number of loading cycles                     |                                                                                                   | unsigned word |
| DisCycle[H/L] | Number of discharging cycles                 |                                                                                                   | unsigned word |
| VBreaker      |                                              | 0 = OK<br>1 = bms stopped charge<br>2 = too high charge current<br>4 = too high discharge current | unsigned byte |
| Charging      | Battery is currently charging                | 1 = charge<br>4 = discharge                                                                       | unsigned byte |

### ECU Status (Read Response 0xAADA)
| Byte (len=10) |   0   |    1     |    2     |   3    |   4    |    5     |   6   |   7   |    8    |   9   |
| ------------- | :---: | :------: | :------: | :----: | :----: | :------: | :---: | :---: | :-----: | :---: |
|               | Mode  | CurrentH | CurrentL | SpeedH | SpeedL | ECU Temp |   ?   |   ?   | Parking |   ?   |


#### Description of the variables
| Variable      | Description         | Unit                | Data Type     |
| ------------- | ------------------- | ------------------- | ------------- |
| Mode          | Speed Mode          | 1 - 3               | unsigned byte |
| Current [H/L] | Current consumption | [0.1 A]             | unsigned word |
| Speed [H/L]   | Current speed       | [0.028 km/h]        | unsigned word |
| ECU Temp      | Temperature of ECU  | Degree Celcius [°C] | signed byte   |
| Parking       | Parking mode        | 2 = on<br>1 = off   | unsigned byte |


### GSM (Read Request 0xBAAA)
| Byte (len=14) |   0   |   1   |   2   |   3   |   4   |   5    |   6   |   7   |   8   |   9   |  10   |  11   |  12   |  13   |
| ------------- | :---: | :---: | :---: | :---: | :---: | :----: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
|               |   ?   |   ?   |   ?   |   ?   | Hour  | Minute |   ?   |   ?   |   ?   |   ?   |   ?   |   ?   |   ?   |   ?   |

#### Description of the variables
| Variable | Description                 | Unit |
| -------- | --------------------------- | ---- |
| Hour     | current hour in localtime   |      |
| Minute   | current minute in localtime |      |


## Additional notes
As @pervolianinen stated in https://github.com/stprograms/SuperSoco485Monitor/issues/2#issuecomment-1676308814, this is a generic protocol that is used in all Lingbo controllers. Using specific hardware converters, the monitor application can be use on these interfaces too. For CAN, this would also need enhancement in how the data is extracted.

# Further projects
This chapter shall contain a list of projects that build upon this project or also target the communication of SuperSoco.

|                          Project                           | Description                                                     |
| :--------------------------------------------------------: | --------------------------------------------------------------- |
| [SuperSoco485](https://github.com/stprograms/SuperSoco485) | Arduino library that implements the specification defined here. |
