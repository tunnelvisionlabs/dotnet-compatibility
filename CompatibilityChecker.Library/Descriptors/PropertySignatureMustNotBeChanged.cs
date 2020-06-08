namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Signature of publicly-accessible property cannot be changed or removed.
    /// </summary>
    internal class PropertySignatureMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertySignatureMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Signature of publicly-accessible property '{0}' was renamed or removed.";
        private static readonly string _category = Categories.Property;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PropertySignatureMustNotBeChanged Instance = new PropertySignatureMustNotBeChanged();

        private PropertySignatureMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string propertyName)
        {
            return new Message(Instance, propertyName);
        }
    }
}
