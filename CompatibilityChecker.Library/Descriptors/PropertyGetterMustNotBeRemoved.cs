namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property getter cannot be removed.
    /// </summary>
    internal class PropertyGetterMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertyGetterMustNotBeRemoved);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible property getter '{0}' cannot be removed.";
        private static new readonly string Category = Categories.Event;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly PropertyGetterMustNotBeRemoved Instance = new ();

        private PropertyGetterMustNotBeRemoved()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string name)
        {
            return new Message(Instance, name);
        }
    }
}
