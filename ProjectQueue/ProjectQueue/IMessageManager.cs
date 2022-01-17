namespace ProjectQueue
{
    public interface IMessageManager
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
