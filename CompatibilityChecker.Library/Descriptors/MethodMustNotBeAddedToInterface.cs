namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Methods should not be added to interfaces.
    /// </summary>
    internal class MethodMustNotBeAddedToInterface : CompatibilityDescriptor
    {
        private const string Id = nameof(MethodMustNotBeAddedToInterface);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Method '{1}' was added to interface '{0}'.";
        private static new readonly string Category = Categories.Interface;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly MethodMustNotBeAddedToInterface Instance = new ();

        private MethodMustNotBeAddedToInterface()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string interfaceName, string methodName)
        {
            return new Message(Instance, interfaceName, methodName);
        }
    }
}
