namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property attributes cannot be changed or removed.
    /// </summary>
    internal class PropertyAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertyAttributesMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible property attributes of '{0}' were changed or removed.";
        private static readonly string _category = Categories.Attribute;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PropertyAttributesMustNotBeChanged Instance = new PropertyAttributesMustNotBeChanged();

        private PropertyAttributesMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string propertyName)
        {
            return new Message(Instance, propertyName);
        }
    }
}
