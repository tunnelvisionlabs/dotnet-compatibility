namespace CompatibilityChecker.Descriptors
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible types cannot be removed. This includes renaming a type or changing the namespace of a type.
    /// An exception to this rule applies for moving a type to another assembly, provided a
    /// <see cref="TypeForwardedToAttribute"/> attribute is applied to the original assembly.
    /// </summary>
    internal class TypeMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(TypeMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "A publicly-accessible type cannot be removed.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly TypeMustNotBeRemoved Instance = new TypeMustNotBeRemoved();

        private TypeMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage()
        {
            return new Message(Instance);
        }
    }
}
