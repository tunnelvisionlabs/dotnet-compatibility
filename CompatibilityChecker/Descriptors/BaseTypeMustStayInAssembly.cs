namespace CompatibilityChecker.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base types cannot be moved out of the assembly.
    /// </summary>
    internal class BaseTypeMustStayInAssembly : CompatibilityDescriptor
    {
        private const string Id = nameof(BaseTypeMustStayInAssembly);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Base type of '{0}' must not be moved outside the assembly.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly BaseTypeMustStayInAssembly Instance = new BaseTypeMustStayInAssembly();

        private BaseTypeMustStayInAssembly()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
