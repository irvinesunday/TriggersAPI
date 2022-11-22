namespace TriggersAPI
{
    public class Subscription
    {
        public SubscriptionProperties SubscriptionProperties { get; set; }
        public string NotificationUrl { get; set; }
        public bool IncludeResourceData { get; set; }
    }
}
