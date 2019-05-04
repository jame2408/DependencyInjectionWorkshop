using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public interface INotification
    {
        void PushNotify(string accountId, string message);
    }

    public class NotifyAdapter : INotification
    {
        public void PushNotify(string accountId, string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(r => { }, accountId, message, "my bot name");
        }
    }
}