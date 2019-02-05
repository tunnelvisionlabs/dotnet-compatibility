namespace CompatibilityChecker.Descriptors
{
    /// <summary>
    /// Publicly-accessible types cannot be changed from stable to preliminary.
    /// </summary>
    internal class TypeMustNotBeMadePreliminaryFromStable : CompatibilityDescriptor
    {
        private const string Id = nameof(TypeMustNotBeMadePreliminaryFromStable);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly visible type '{0}' changed from stable to preliminary.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly TypeMustNotBeMadePreliminaryFromStable Instance = new TypeMustNotBeMadePreliminaryFromStable();

        private TypeMustNotBeMadePreliminaryFromStable()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
