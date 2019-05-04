namespace DependencyInjectionWorkshop.Adapters
{
    public class LogAdapter
    {
        public void LogFailedCount(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"Verify Failed. AccountId: {accountId}, Failed Times: {failedCount}");
        }
    }
}