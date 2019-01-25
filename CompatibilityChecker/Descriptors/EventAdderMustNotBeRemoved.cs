namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible events cannot have their adder removed.
    /// </summary>
    internal class EventAdderMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(EventAdderMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible event '{0}' cannot have its adder removed.";
        private static readonly string _category = Categories.Event;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly EventAdderMustNotBeRemoved Instance = new EventAdderMustNotBeRemoved();

        private EventAdderMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
