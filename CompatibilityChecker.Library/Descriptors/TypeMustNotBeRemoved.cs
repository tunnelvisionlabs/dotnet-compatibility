namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible types cannot be removed. This includes renaming a type or changing the namespace of a type.
    /// An exception to this rule applies for moving a type to another assembly, provided a
    /// <see cref="TypeForwardedToAttribute"/> attribute is applied to the original assembly.
    /// </summary>
    internal class TypeMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(TypeMustNotBeRemoved);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible type '{0}' cannot be removed.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly TypeMustNotBeRemoved Instance = new ();

        private TypeMustNotBeRemoved()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
