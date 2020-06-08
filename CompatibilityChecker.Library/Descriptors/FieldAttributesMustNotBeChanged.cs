namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible field attributes cannot be changed or removed.
    /// </summary>
    internal class FieldAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(FieldAttributesMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible field attributes of '{0}' were changed or removed.";
        private static readonly string _category = Categories.Attribute;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly FieldAttributesMustNotBeChanged Instance = new FieldAttributesMustNotBeChanged();

        private FieldAttributesMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string fieldName)
        {
            return new Message(Instance, fieldName);
        }
    }
}
