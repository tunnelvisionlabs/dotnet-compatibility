namespace CompatibilityChecker.Library.Descriptors
{
    /// <summary>
    /// Attributes of publicly-accessible types must not be changed.
    /// </summary>
    internal class PublicAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PublicAttributesMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Attributes of publicly visible type '{0}' changed.";
        private static new readonly string Category = Categories.Attribute;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly PublicAttributesMustNotBeChanged Instance = new ();

        private PublicAttributesMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
