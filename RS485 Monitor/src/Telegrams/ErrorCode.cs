public class ErrorCode{
    private UInt16 raw;

    [Flags]
    public enum ErrorBits {
        CTRL_DISCONNECT_99 = 0x0001,
        CTRL_ERROR_98 = 0x0002,
        CTRL_ERROR_97 = 0x0004,
        CTRL_ERROR_96 = 0x0008,
        CTRL_ERROR_95 = 0x0010,
        /// <summary>
        /// Battery is disconnected
        /// </summary>
        BATTERY_DISCONNECT_94 = 0x0020,
        /// <summary>
        /// Charging current too high
        /// </summary>
        BATTERY_CHARGE_CURRENT_93 = 0x0040,
        /// <summary>
        /// Charging has stopped
        /// </summary>
        BATTERY_CHARGE_STOPPED_92 = 0x0080,
        /// <summary>
        /// Battery over temperature
        /// </summary>
        BATTERY_OVERTEMP_91 = 0x0100,
        /// <summary>
        /// Discharge current too high
        /// </summary>
        BATTERY_DISCHARGE_CURRENT_90 = 0x0200,
        BATTERY_ERROR_89 = 0x0400,
        BATTERY_ERROR_88 = 0x0800,
    }

    public ErrorCode(int raw) {
        this.raw = (UInt16)raw;
    }

    public ErrorBits State { get => (ErrorBits)this.raw; }

}
