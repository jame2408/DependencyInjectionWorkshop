namespace DependencyInjectionWorkshop.Adapters
{
    public interface ILogger
    {
        void Info(string accountId, int failedCount);
    }

    public class LogAdapter : ILogger
    {
        public void Info(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"Verify Failed. AccountId: {accountId}, Failed Times: {failedCount}");
        }
    }
}