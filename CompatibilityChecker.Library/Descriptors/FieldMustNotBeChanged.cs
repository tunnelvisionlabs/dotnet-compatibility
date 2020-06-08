namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible field cannot be changed or removed.
    /// </summary>
    internal class FieldMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(FieldMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible field '{0}' was renamed or removed.";
        private static readonly string _category = Categories.Field;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly FieldMustNotBeChanged Instance = new FieldMustNotBeChanged();

        private FieldMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string fieldName)
        {
            return new Message(Instance, fieldName);
        }
    }
}
