namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible event attributes cannot be changed or removed.
    /// </summary>
    internal class EventAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(EventAttributesMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible event attributes of '{0}' were changed or removed.";
        private static new readonly string Category = Categories.Attribute;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly EventAttributesMustNotBeChanged Instance = new ();

        private EventAttributesMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
