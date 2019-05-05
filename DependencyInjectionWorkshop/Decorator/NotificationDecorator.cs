using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorator
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authentication = authenticationService;
            _notification = notification;
        }

        private void NotificationVerify(string accountId)
        {
            _notification.PushMessage(accountId, $"accountId:{accountId} verify failed.");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                NotificationVerify(accountId);
            }

            return isValid;
        }
    }
}