using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public interface INotification
    {
        void PushMessage(string accountId, string message);
    }

    public class NotifyAdapter : INotification
    {
        public void PushMessage(string accountId, string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(r => { }, accountId, message, "my bot name");
        }
    }
}