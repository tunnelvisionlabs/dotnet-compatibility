namespace CompatibilityChecker.Library.Descriptors
{
    /// <summary>
    /// The <c>sealed</c> modifier must not be added to a publicly-accessible type, unless that type does not have any
    /// publicly-accessible constructors.
    /// </summary>
    internal class SealedMustNotBeAddedToType : CompatibilityDescriptor
    {
        private const string Id = nameof(SealedMustNotBeAddedToType);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "The 'sealed' modifier cannot be added to type '{0}'.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly SealedMustNotBeAddedToType Instance = new ();

        private SealedMustNotBeAddedToType()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
