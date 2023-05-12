/// <summary>
/// Interface for classes that can visualize telegrams to the user
/// </summary>
public interface IUserVisualizable
{
    /// <summary>
    /// Print the telegram to the user
    /// </summary>
    /// <param name="tg">Telegram to print</param>
    public void PrintTelegram(BaseTelegram tg);
    
    /// <summary>
    /// Flush all outgoing data
    /// </summary>
    public void Flush();
}