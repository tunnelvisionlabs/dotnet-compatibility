namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible events cannot be removed.
    /// </summary>
    internal class EventMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(EventMustNotBeRemoved);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible event '{0}' cannot be removed.";
        private static new readonly string Category = Categories.Event;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly EventMustNotBeRemoved Instance = new ();

        private EventMustNotBeRemoved()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
