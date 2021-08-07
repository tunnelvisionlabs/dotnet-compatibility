namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Implemented interfaces cannot be removed. This includes renaming an interface.
    /// </summary>
    internal class ImplementedInterfaceMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(ImplementedInterfaceMustNotBeRemoved);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Implemented interface '{1}' cannot be removed from '{0}'.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly ImplementedInterfaceMustNotBeRemoved Instance = new ();

        private ImplementedInterfaceMustNotBeRemoved()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName, string interfaceName)
        {

            return new Message(Instance, typeName, interfaceName);
        }
    }
}
