using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public class NotifyAdapter
    {
        public void Notify(string accountId, string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(r => { }, accountId, message, "my bot name");
        }
    }
}