namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible method cannot be changed or removed.
    /// </summary>
    internal class MethodMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(MethodMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Publicly-accessible method '{0}' was changed or removed.";
        private static new readonly string Category = Categories.Method;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly MethodMustNotBeChanged Instance = new ();

        private MethodMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string methodName)
        {
            return new Message(Instance, methodName);
        }
    }
}
