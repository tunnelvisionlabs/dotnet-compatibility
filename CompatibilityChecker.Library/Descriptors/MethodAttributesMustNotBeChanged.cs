namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible method attributes cannot be changed or removed.
    /// </summary>
    internal class MethodAttributesMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(MethodAttributesMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible method attributes '{0}' were renamed or removed.";
        private static new readonly string Category = Categories.Attribute;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly MethodAttributesMustNotBeChanged Instance = new ();

        private MethodAttributesMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string methodName)
        {
            return new Message(Instance, methodName);
        }
    }
}
