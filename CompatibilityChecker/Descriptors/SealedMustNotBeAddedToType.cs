namespace CompatibilityChecker.Descriptors
{
    /// <summary>
    /// The <c>sealed</c> modifier must not be added to a publicly-accessible type, unless that type does not have any
    /// publicly-accessible constructors.
    /// </summary>
    internal class SealedMustNotBeAddedToType : CompatibilityDescriptor
    {
        private const string Id = nameof(SealedMustNotBeAddedToType);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "The 'sealed' modifier cannot be added to type '{0}'.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly SealedMustNotBeAddedToType Instance = new SealedMustNotBeAddedToType();

        private SealedMustNotBeAddedToType()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
