namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Implemented interfaces cannot be removed. This includes renaming an interface.
    /// </summary>
    internal class ImplementedInterfaceMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(ImplementedInterfaceMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Implemented interface '{1}' cannot be removed from '{0}'.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly ImplementedInterfaceMustNotBeRemoved Instance = new ImplementedInterfaceMustNotBeRemoved();

        private ImplementedInterfaceMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string typeName, string interfaceName)
        {
            

            return new Message(Instance, typeName, interfaceName);
        }
    }
}
