namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Signature of publicly-accessible property cannot be changed or removed.
    /// </summary>
    internal class PropertySignatureMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertySignatureMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Signature of publicly-accessible property '{0}' was renamed or removed.";
        private static new readonly string Category = Categories.Property;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly PropertySignatureMustNotBeChanged Instance = new ();

        private PropertySignatureMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string propertyName)
        {
            return new Message(Instance, propertyName);
        }
    }
}
