namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Publicly-accessible property setter cannot be removed.
    /// </summary>
    internal class PropertySetterMustNotBeRemoved : CompatibilityDescriptor
    {
        private const string Id = nameof(PropertySetterMustNotBeRemoved);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Publicly-accessible property setter '{0}' cannot be removed.";
        private static readonly string _category = Categories.Event;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PropertySetterMustNotBeRemoved Instance = new PropertySetterMustNotBeRemoved();

        private PropertySetterMustNotBeRemoved()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string name)
        {
            return new Message(Instance, name);
        }
    }
}
