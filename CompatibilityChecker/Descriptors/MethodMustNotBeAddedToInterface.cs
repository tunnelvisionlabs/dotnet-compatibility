namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Methods should not be added to interfaces.
    /// </summary>
    internal class MethodMustNotBeAddedToInterface : CompatibilityDescriptor
    {
        private const string Id = nameof(MethodMustNotBeAddedToInterface);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Method '{1}' was added to interface '{0}'.";
        private static readonly string _category = Categories.Interface;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly MethodMustNotBeAddedToInterface Instance = new MethodMustNotBeAddedToInterface();

        private MethodMustNotBeAddedToInterface()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string interfaceName, string methodName)
        {
            return new Message(Instance, interfaceName, methodName);
        }
    }
}
