using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Proxy;

namespace DependencyInjectionWorkshop.Decorator
{
    public class FailedCounterDecorator :IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter)
        {
            _authentication = authenticationService;
            _failedCounter = failedCounter;
        }

        private void CheckAccountIsLocked(string accountId)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);
            var isValid = _authentication.Verify(accountId, password, otp);

            if (isValid)
            {
                _failedCounter.Reset(accountId);
            }
            else
            {
                _failedCounter.Add(accountId);
            }

            return isValid;
        }
    }
}