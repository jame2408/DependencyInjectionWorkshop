using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Proxy;

namespace DependencyInjectionWorkshop.Decorator
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter) : base(authenticationService)
        {
            _failedCounter = failedCounter;
        }

        private void CheckAccountIsLocked(string accountId)
        {
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);
            var isValid = base.Verify(accountId, password, otp);

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