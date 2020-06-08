namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible events cannot have their remover removed.
    /// </summary>
    internal class EventRemoverMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(EventRemoverMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible event '{0}' cannot have its remover removed.";
        private static readonly string _category = Categories.Event;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly EventRemoverMustNotBeRemoved Instance = new EventRemoverMustNotBeRemoved();

        private EventRemoverMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
