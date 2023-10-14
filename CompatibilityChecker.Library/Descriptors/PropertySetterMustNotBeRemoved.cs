namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property setter cannot be removed.
    /// </summary>
    internal class PropertySetterMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertySetterMustNotBeRemoved);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible property setter '{0}' cannot be removed.";
        private static new readonly string Category = Categories.Event;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly PropertySetterMustNotBeRemoved Instance = new ();

        private PropertySetterMustNotBeRemoved()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string name)
        {
            return new Message(Instance, name);
        }
    }
}
