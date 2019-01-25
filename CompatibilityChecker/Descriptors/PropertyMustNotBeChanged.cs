namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property cannot be changed or removed.
    /// </summary>
    internal class PropertyMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertyMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible property '{0}' was renamed or removed.";
        private static readonly string _category = Categories.Property;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PropertyMustNotBeChanged Instance = new PropertyMustNotBeChanged();

        private PropertyMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string propertyName)
        {
            return new Message(Instance, propertyName);
        }
    }
}
