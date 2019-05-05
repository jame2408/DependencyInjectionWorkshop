using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Decorator
{
    public class NotificationDecorator : AuthenticationBaseDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification) : base(authenticationService)
        {
            _notification = notification;
        }

        private void NotificationVerify(string accountId)
        {
            _notification.PushMessage(accountId, $"accountId:{accountId} verify failed.");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                NotificationVerify(accountId);
            }

            return isValid;
        }
    }
}