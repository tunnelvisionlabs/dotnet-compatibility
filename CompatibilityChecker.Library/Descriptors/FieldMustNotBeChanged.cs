namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible field cannot be changed or removed.
    /// </summary>
    internal class FieldMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(FieldMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible field '{0}' was renamed or removed.";
        private static new readonly string Category = Categories.Field;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly FieldMustNotBeChanged Instance = new ();

        private FieldMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string fieldName)
        {
            return new Message(Instance, fieldName);
        }
    }
}
