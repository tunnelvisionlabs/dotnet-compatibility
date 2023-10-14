namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible events cannot have their raiser removed.
    /// </summary>
    internal class EventRaiserMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(EventRaiserMustNotBeRemoved);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible event '{0}' cannot have its raiser removed.";
        private static new readonly string Category = Categories.Event;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly EventRaiserMustNotBeRemoved Instance = new ();

        private EventRaiserMustNotBeRemoved()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
