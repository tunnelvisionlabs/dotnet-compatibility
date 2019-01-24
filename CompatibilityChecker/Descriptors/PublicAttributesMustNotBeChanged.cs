namespace CompatibilityChecker.Descriptors
{
    /// <summary>
    /// Attributes of publicly-accessible types must not be changed.
    /// </summary>
    internal class PublicAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PublicAttributesMustNotBeChanged);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Attributes of publicly visible type changed.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PublicAttributesMustNotBeChanged Instance = new PublicAttributesMustNotBeChanged();

        private PublicAttributesMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage()
        {
            return new Message(Instance);
        }
    }
}
