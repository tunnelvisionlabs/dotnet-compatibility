namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible events cannot be removed.
    /// </summary>
    internal class EventMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(EventMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible event '{0}' cannot be removed.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly EventMustNotBeRemoved Instance = new EventMustNotBeRemoved();

        private EventMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
