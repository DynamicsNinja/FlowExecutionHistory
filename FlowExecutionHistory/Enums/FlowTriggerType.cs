namespace Fic.XTB.FlowExecutionHistory.Enums
{
    public static class FlowTriggerType
    {
        public const string Automated = "OpenApiConnectionWebhook";
        public const string Scheduled = "Recurrence";
        public const string Instant = "Request";
    }
}
