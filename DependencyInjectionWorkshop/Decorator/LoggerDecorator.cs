using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Proxy;

namespace DependencyInjectionWorkshop.Decorator
{
    public class LoggerDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LoggerDecorator(IAuthentication authenticationService, ILogger logger, IFailedCounter failedCounter)
        {
            _authentication = authenticationService;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void LogVerify(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"Verify Failed. AccountId: {accountId}, Failed Times: {failedCount}");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }
}