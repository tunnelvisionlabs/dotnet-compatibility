namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property attributes cannot be changed or removed.
    /// </summary>
    internal class PropertyAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertyAttributesMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible property attributes of '{0}' were changed or removed.";
        private static new readonly string Category = Categories.Attribute;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly PropertyAttributesMustNotBeChanged Instance = new ();

        private PropertyAttributesMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string propertyName)
        {
            return new Message(Instance, propertyName);
        }
    }
}
