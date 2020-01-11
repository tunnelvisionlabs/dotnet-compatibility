namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property getter cannot be removed.
    /// </summary>
    internal class PropertyGetterMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertyGetterMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible property getter '{0}' cannot be removed.";
        private static readonly string _category = Categories.Event;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PropertyGetterMustNotBeRemoved Instance = new PropertyGetterMustNotBeRemoved();

        private PropertyGetterMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string name)
        {
            return new Message(Instance, name);
        }
    }
}
