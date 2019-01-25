namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible event attributes cannot be changed or removed.
    /// </summary>
    internal class EventAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(EventAttributesMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible event attributes of '{0}' were changed or removed.";
        private static readonly string _category = Categories.Attribute;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly EventAttributesMustNotBeChanged Instance = new EventAttributesMustNotBeChanged();

        private EventAttributesMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
