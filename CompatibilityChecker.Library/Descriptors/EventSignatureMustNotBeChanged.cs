namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible event signature cannot be changed or removed.
    /// </summary>
    internal class EventSignatureMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(EventAttributesMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible event signature of '{0}' was changed or removed.";
        private static readonly string _category = Categories.Event;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly EventSignatureMustNotBeChanged Instance = new EventSignatureMustNotBeChanged();

        private EventSignatureMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string eventName)
        {
            return new Message(Instance, eventName);
        }
    }
}
