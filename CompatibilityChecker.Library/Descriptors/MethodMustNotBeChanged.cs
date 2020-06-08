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
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible method '{0}' was changed or removed.";
        private static readonly string _category = Categories.Method;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly MethodMustNotBeChanged Instance = new MethodMustNotBeChanged();

        private MethodMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string methodName)
        {
            return new Message(Instance, methodName);
        }
    }
}
