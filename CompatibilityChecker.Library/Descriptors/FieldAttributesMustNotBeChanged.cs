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
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible field attributes of '{0}' were changed or removed.";
        private static new readonly string Category = Categories.Attribute;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly FieldAttributesMustNotBeChanged Instance = new ();

        private FieldAttributesMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string fieldName)
        {
            return new Message(Instance, fieldName);
        }
    }
}
