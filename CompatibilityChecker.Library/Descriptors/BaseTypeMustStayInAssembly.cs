namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base types cannot be moved out of the assembly.
    /// </summary>
    internal class BaseTypeMustStayInAssembly : CompatibilityDescriptor
    {
        private const string Id = nameof(BaseTypeMustStayInAssembly);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Base type of '{0}' must not be moved outside the assembly.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly BaseTypeMustStayInAssembly Instance = new ();

        private BaseTypeMustStayInAssembly()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string typeName)
        {
            return new Message(Instance, typeName);
        }
    }
}
