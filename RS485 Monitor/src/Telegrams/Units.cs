/// <summary>
/// List of available units in the system
/// </summary>
public enum Units
{
    /// <summary>
    /// Main ECU / bus master
    /// </summary>
    ECU = 0xAA,
    /// <summary>
    /// Engine controller
    /// </summary>
    ENGINE_CONTROLLER = 0xDA,
    /// <summary>
    /// Battery management system
    /// </summary>
    BATTERY = 0x5A,
    /// <summary>
    /// Speedometer
    /// </summary>
    SPEEDOMETER = 0xBA
}
