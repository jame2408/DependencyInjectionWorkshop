﻿namespace DependencyInjectionWorkshop.Adapters
{
    public interface ILogger
    {
        void Info(string message);
    }

    public class LogAdapter : ILogger
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}