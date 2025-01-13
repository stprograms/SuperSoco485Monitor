[![.NET Workflow for test building](https://github.com/stprograms/SuperSoco485Monitor/actions/workflows/build.yml/badge.svg)](https://github.com/stprograms/SuperSoco485Monitor/actions/workflows/build.yml)

# SuperSoco485Monitor
C# Application to monitor the communication on the internal RS485 Bus on SuperSoco Motorcycles

This small application is used for debug and analysis of the data received from the RS485 bus of SuperSoco Motorcycles. It is not compatible with newer motorcycles that use CAN bus as the communication medium. Since this application is a helper for first analysis, the code is more or less written quick and dirty.

Information about the baud rate, Structure of the telegrams and content of the telegrams has been derived from the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) of [Xmanu12](https://github.com/Xmanu12).

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
SuperSoco (at least TC Max), uses a JST SM 4 pin connector under the seat. This connector can be disconnected if the motorcycle is not charged through the external charger port. Instead of the connection to the external charger, a JST SM 4 pin plug can be connected and attached to a RS485 converter. The pinning of the connector is as following:

| Pin |    Signal    |
| --- | :----------: |
| 1   | brake signal |
| 2   |   RX- / B    |
| 3   |   RX+ / A    |
| 4   |    Ground    |

The brake signal is used to prevent movement when the charger is plugged in.

# RS485 Protocol
The following information is taken from the [Dashboard Android App](https://github.com/Xmanu12/SuSoDevs) project and is summarized here. All the information has been reverse engineered and can therefor hold errors and unknown data.

The communication is using **9600 Baud**, with 8 bit data and 1 stop bit. It is based on Read requests and Read responses and data is transmitted in telegrams.

## Testing Method

### Including but not limited to the following approaches:

#### Unplugging and Re-plugging Physical Wiring:
Dynamically unplug and re-plug the RS485 physical wiring to test the system's response during communication interruptions and recovery.
#### Removing Components and Simulating Communication Packets:
Remove a component and use a simulation tool to generate communication packets for the removed component. Test how the other components react to the simulated packets.
#### Operating the Vehicle and Monitoring Packets:
Operate the vehicle (e.g., accelerate, decelerate) and monitor the RS485 communication data to verify that the packets correspond to changes in the vehicle’s status.
#### Triggering Vehicle Status via External Input:
Use external devices (such as controlling the Power Supply to replace the battery) to simulate specific vehicle conditions, such as triggering undervoltage, and monitor the system’s response.

## System and Unit
This system is a two-wire RS485 communication system and includes four different units:
| Unit      |Description| 
| :---------|:----------------------|
|ECU        |Master,<br/> sending request,receiving responses, consolidating system information.|
|SpeedMeter |Displays the vehicle's status.<br/> If the ECU is absent from the system,the SpeedMeter takes over as the Master,<br/>  but it only sends out requests without altering its state based on the responses received.|
|Controller |Control Motor,Returns Response. |
|Battery    |Power,Returns Response.|

## Generic information and structure

Each telegram starts with two bytes specifying a request or a response, followed by one byte source (id) and one byte destination (id). After that, the length of the user data in Bytes and the data itself is transmitted. Lastly, a one byte checksum and the end tag 0x0D terminates the telegram.

| Byte |   0   |   1   |   2    |   3   |    4    |  4 + 1  |  4 + 2  |  ...  |   4 + n   | 4 + n + 1 | 4 + n + 2 |
| :--- | :---: | :---: | :----: | :---: | :-----: | :-----: | :-----: | :---: | :-------: | :-------: | :-------: |
|      | type1 | type2 | dest | source  | len (n) | data[0] | data[1] |  ...  | data[n-1] | checksum  |   0x0D    |

There are two known combinations for the type:
| Byte0 | Byte1 | Type          |
| ----: | :---- | ------------- |
|  0xC5 | 0x5C  | Request  |
|  0xB6 | 0x6B  | Response |

Four types addr::
| Addr   | Type     |
| ----:  | :------- |
| 0xAA   |Master    |
| 0xBA   |Speedometer|
| 0xDA   |Controller|
| 0x5A   |Battery   |

Six Types Package:

| Package Type     | Dest       | Src        | PduLen  |
| :--------------  | :--------  | :--------- |:------- |
| Speedometer_Req  | SpeedMeter | Master     | 14      |
| Speedometer_Res  | Master     | SpeedMeter | 1       |
| Controller_Req   | Controller | Master     | 2       |
| Controller_Res   | Master     | Controller | 10      |
| Battery_Req      | Battery    | Master     | 1       |
| Battery_Res      | Master     | Battery    | 10      |

## Checksum calculation
The checksum byte is calculated by making an XOR calculation of the data Bytes and the length Byte. The following C# example shall deepen the understanding how the checksum is calculated.

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

### SpeedMeter 
![image](res/speedometer.png)

#### Req

| Byte (len=14) |  0  |            1           |       2       |         3       |  4  |  5  |     6    |     7    |       8       |      9      |       10          |         11         | 12  |      13       |
| ------------- | :-: | :--------------------: | :-----------: | :-------------: | :-: | :-: | :------: | :------: | :-----------: | :---------: | :---------------: | :----------------: | :-: | :-----------: |
|               | Soc | Controller<br/>Current | SpeedDisplay  | Controller Temp | ?   |  ?  | Errcode0 | Errcode1 | Vehicle State | GearDisplay |Speed From Ctrl H|Speed From Ctrl L |  ?  |Remaining Range|

##### Description of the variables
| Variable                    | Description                                  
| --------------------------- | -------------------------------------------- 
| Soc                         | Battery percentage display ,Unit=%, 0~100  Soc>100 wil not display. value = last Battery[Soc] received.
| CtrlCurrent                 | 0x00 ~ 0x19 =  0 ~ 30A, Unit = 2.5A ,Related to Ctrl[Current].if(Ctrl[Current]>30A)CtrlCurrent = 30,else CtrlCurrent = Ctrl[Current];
| SpeedDisplay                | speed(km/h) 0~127(estimated value), if(speed>127)The odometer in Speedometer will treat values greater than 127 as 127.</br> Related to Ctrl[Speed], SpeedDisplay = Ctrl[speed]*0.11
| CtlTempDisplay              | 0~9,if(CtlTempDisplay>10)Will not display on speedometer.CtlTempDisplay = Ctrl[temp]/20.
| Errcode                     | See the Error Code Table for details.                                            
| Vehicle State               | Charging = 4, Parking=1,  else = 0, [Something?] = 10 . [Something?] related to Ctrl[UnKnown State] . 
| GearDisplay                 | = Ctrl[Gear]. 0~3, if(GearDisplay>3)will not display.
| Speed From Ctrl             | = Ctrl[Speed]             
| Remaining Range             | Soc*F(Gear), F(x) =1-((x-1)*0.2), Gear=1,2,3, if(Battery disconnect) = 0
                
#### Res

| Byte (len=1)  |    0    |
| ------------- | :-----: |
|               |    ?    |

### Controller
#### Req
| Byte (len=2)  |    0    |       1        |
| ------------- | :-----: | :------------: |
|               |    ?    | Charging State |
##### Description of the variables
| Variable                    | Description                                  
| --------------------------- | -------------------------------------------- 
|  Charging State             |if(Battery[Charging] == 1) Charging State = 1; if Ctrl received (Charging State = 1), motor can't start running.

#### Res

| Byte (len=10) |      0     |      1     |      2     |     3    |    4     |   5    |     6     |       7       |    8     |    9     |  
| ------------- | :--------: |----------: | :--------: | :------: | :------: | :----: | :-------: | :-----------: | :------: | :------: |
|               |    Gear    | Current(H) | Current(L) | Speed(H) | Speed(L) |  Temp  | ErrorCode | Unknown State |  Parking |    ?     |

##### Description of the variables
| Variable                    | Description                                  
| --------------------------- | -------------------------------------------- 
| Gear | change by (Mode switch on Right Switch)  =  1,2,3 
| Current | 0.1A
| Speed | 0.028 km/h
| Temp |Ctrl Temp °C
| ErrorCode | See the Error Code Table for details
|Unknown State |Something? = 2, will change Vehicle State to 0x10
|Parking |change by (Side Stand), Parking = 2,else = 1

### Battery
#### Req
| Byte (len=1)  |    0    |
| ------------- | :-----: | 
|               |    ?    | 
#### Res

| Byte (len=10) |      0     |      1     |      2     |     3    |    4     |      5     |      6      |       7       |      8      |    9     |  
| ------------- | :--------: |----------: | :--------: | :------: | :------: | :--------: | :---------: | :-----------: | :---------: | :------: |
|               |    Volt    |     Soc    |    Temp    |  Current | Cycle(H) |  Cycle(L)  | Discycle(H) |  Discycle(L)  |  Error Code | Charging |

##### Description of the variables
| Variable  | Unit | Description |
| --------- | -----|--------------------------------------- |
| Volt      | V | |
| Soc       | % | |
|Temp       | °C| |
|Current    | A | if(Current<0)means discharging Current. |
|Cycle      | - | Number of loading cycles |
|Discycle   | - | Number of discharging cycles|
|ErrorCode  | - | See the Error Code Table for details |
|Charging   | - | Charging = 1 , (Discharging = 4?,not observed.Cause BMS diff?) |

### Error Codes

| Byte | Bit | ErrorCode | Report Unit | Report Bit |Other |
|----- | ----|-----------|-------------|------------|----|
|6|0|99|Ctrl|Disconnect||
|6|1|98|Ctrl|Ctrl(6,1|2|4|5)|over current?</br> motor blocking= bit5</br> under voltage = bit4</br> over temperature? |
|6|2|97|Ctrl|Ctrl(6,0) | |
|6|3|96|Ctrl|Ctrl(6,0) | |
|6|4|95|Ctrl|Ctrl(6,6) | |
|6|5|94|Battery|Disconnect | |
|6|6|93|Battery|Battery(8,1) ||
|6|7|92|Battery|Battery(8,0) ||
|7|0|91|Battery|Battery[Temp]>=3B(60°C) ||
|7|1|90|Battery|Battery(8,2) ||
|7|2|89|Battery|Battery(8,5) ||
|7|3|88|Battery|Battery(8,7)||
|7|4|87|X|||
|7|5|86|X|||

*Ctrl(1,2)means the bit is at Ctrl pdu byte 1,bit 2

## Additional notes
As @pervolianinen stated in https://github.com/stprograms/SuperSoco485Monitor/issues/2#issuecomment-1676308814, this is a generic protocol that is used in all Lingbo controllers. Using specific hardware converters, the monitor application can be use on these interfaces too. For CAN, this would also need enhancement in how the data is extracted.

# Further projects
This chapter shall contain a list of projects that build upon this project or also target the communication of SuperSoco.

|                          Project                           | Description                                                     |
| :--------------------------------------------------------: | --------------------------------------------------------------- |
| [SuperSoco485](https://github.com/stprograms/SuperSoco485) | Arduino library that implements the specification defined here. |
