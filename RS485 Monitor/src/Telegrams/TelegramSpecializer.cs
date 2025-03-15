using NLog;

namespace RS485Monitor.Telegrams
{
    /// <summary>
    /// Static helper class to specialize telegrams
    /// </summary>
    public class TelegramSpecializer
    {

        /// <summary>
        /// Dictionary of known telegram types
        /// </summary>
        protected static Dictionary<UInt16, Type> knownTelegrams = new()
        {
            {ControllerRequest.TELEGRAM_ID, typeof(ControllerRequest) },
            {ControllerResponse.TELEGRAM_ID, typeof(ControllerResponse) },
            {BatteryRequest.TELEGRAM_ID, typeof(BatteryRequest) },
            {BatteryResponse.TELEGRAM_ID, typeof(BatteryResponse) },
            {SpeedometerRequest.TELEGRAM_ID, typeof(SpeedometerRequest) },
            {SpeedometerResponse.TELEGRAM_ID, typeof(SpeedometerResponse) },
        };

        /// <summary>
        /// Class logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Try to get the specialization of a telegram. If the telegram cannot
        /// be converted to a specialized type, the original telegram is returned.
        /// </summary>
        /// <param name="telegram"></param>
        /// <returns></returns>
        public static BaseTelegram Specialize(BaseTelegram telegram)
        {
            // try to fetch the special telegram type
            if (knownTelegrams.TryGetValue(telegram.Id, out Type? specialType))
            {
                var tg = (BaseTelegram?)Activator.CreateInstance(specialType, [telegram]);
                if (tg != null)
                {
                    return tg;
                }
            }
            else
            {
                logger.Error("Unknown telegram type: {0}", telegram.Id);
            }

            return telegram;
        }
    }
}
