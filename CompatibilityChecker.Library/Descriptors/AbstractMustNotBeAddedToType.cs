namespace CompatibilityChecker.Library.Descriptors
{
    /// <summary>
    /// The <c>sealed</c> modifier must not be added to a publicly-accessible type, unless that type does not have any
    /// publicly-accessible constructors.
    /// </summary>
    internal class AbstractMustNotBeAddedToType : CompatibilityDescriptor
    {
        private const string Id = nameof(AbstractMustNotBeAddedToType);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "The 'abstract' modifier cannot be added to type '{0}'.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly AbstractMustNotBeAddedToType Instance = new AbstractMustNotBeAddedToType();

        private AbstractMustNotBeAddedToType()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
