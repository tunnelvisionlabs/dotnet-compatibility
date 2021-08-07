namespace CompatibilityChecker.Library.Descriptors
{
    /// <summary>
    /// Publicly-accessible types cannot be changed from stable to preliminary.
    /// </summary>
    internal class TypeMustNotBeMadePreliminaryFromStable : CompatibilityDescriptor
    {
        private const string Id = nameof(TypeMustNotBeMadePreliminaryFromStable);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly visible type '{0}' changed from stable to preliminary.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly TypeMustNotBeMadePreliminaryFromStable Instance = new ();

        private TypeMustNotBeMadePreliminaryFromStable()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
