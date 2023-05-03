# SuperSoco485Monitor
C# Application to monitor the communication on the internal RS485 Bus on Super Soco Motorcycles

This small application is used for debug and analysis of the data received from the RS485 bus of Super Soco Motorcycles. It is not compatible with newer motorcycles that use CAN bus as the communication medium. Since this application is a helper for first analysis, the code is more or less written quick and dirty.

Information about the Baudrate, Structure of the telegrams and content of the telegrams has been derived from the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) of [Xmanu12](https://github.com/Xmanu12).


## Necessary hardware
For connecting to the internal RS485 bus of the motorcycle, you need a RS485 to serial converter. You can use all possible versions, like
- RS485 to RS232 com port
- RS485 to USB
- RS485 to bluetooth like mentioned in the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) project under documents.

The best way to connect the RS485 is the 4 pin JST SM connector under the seat that connects the internals and the external battery plug. There, the two middle connectors are the RS485 interface.

> *Todo*:
> Better describe the connection to the interface

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

| Byte (len=10) |    0    |   1   |   2   |   3    |   4   |   5   |   6    |   7    |   8   |    9     |
| ------------- | :-----: | :---: | :---: | :----: | :---: | :---: | :----: | :----: | :---: | :------: |
|               | Voltage |  SoC  | Temp  | Charge |   ?   |   ?   | CycleH | CycleL |   ?   | Charging |

#### Description of the variables
| Variable   | Description                                  | Unit          |
| ---------- | -------------------------------------------- | ------------- |
| Voltage    | current Voltage of the Battery in V          | Volts[V]      |
| SoC        | State of Charge in %                         | Percent [%]   |
| Temp       | current temperatur of the BMS                | Degree C [°C] |
| Charge     | current charging or discharging current in A | Ampere [A]    |
| Cycle[H/L] | Number of loading cycles                     |               |
| Charging   | Battery is currently charging                |               |

### ECU Status (Read Response 0xAADA)
| Byte (len=10) |   0   |    1     |    2     |   3    |   4    |    5     |   6   |   7   |    8    |   9   |
| ------------- | :---: | :------: | :------: | :----: | :----: | :------: | :---: | :---: | :-----: | :---: |
|               | Mode  | CurrentH | CurrentL | SpeedH | SpeedL | ECU Temp |   ?   |   ?   | Parking |   ?   |


#### Description of the variables
| Variable      | Description                 | Unit                |
| ------------- | --------------------------- | ------------------- |
| Mode          | Speed Mode (1-3)            |                     |
| Current [H/L] | Current consumption         | [mA] ?              |
| Speed [H/L]   | Current speed               | [km/h]              |
| ECU Temp      | Temperature of ECU          | Degree Celcius [°C] |
| Parking       | Parking mode (2=on / 1=off) |                     |
