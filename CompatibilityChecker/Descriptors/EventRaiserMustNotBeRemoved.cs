namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible events cannot have their raiser removed.
    /// </summary>
    internal class EventRaiserMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(EventRaiserMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible event '{0}' cannot have its raiser removed.";
        private static readonly string _category = Categories.Event;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly EventRaiserMustNotBeRemoved Instance = new EventRaiserMustNotBeRemoved();

        private EventRaiserMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
