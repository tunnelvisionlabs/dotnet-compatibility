namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base types cannot be changed. This includes renaming a type or changing the namespace of a type.
    /// </summary>
    internal class BaseTypeMustNotChange : CompatibilityDescriptor
    {
        private const string Id = nameof(BaseTypeMustNotChange);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Base type of '{0}' must not be changed.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly BaseTypeMustNotChange Instance = new BaseTypeMustNotChange();

        private BaseTypeMustNotChange()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
