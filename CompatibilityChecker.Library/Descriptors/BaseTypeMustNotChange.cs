namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base types cannot be changed. This includes renaming a type or changing the namespace of a type.
    /// </summary>
    internal class BaseTypeMustNotChange : CompatibilityDescriptor
    {
        private const string Id = nameof(BaseTypeMustNotChange);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Base type of '{0}' must not be changed.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly BaseTypeMustNotChange Instance = new ();

        private BaseTypeMustNotChange()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
