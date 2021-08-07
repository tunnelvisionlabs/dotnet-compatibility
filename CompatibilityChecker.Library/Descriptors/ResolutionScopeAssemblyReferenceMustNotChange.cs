namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Resolution scope assembly references cannot be changed.
    /// </summary>
    internal class ResolutionScopeAssemblyReferenceMustNotChange : CompatibilityDescriptor
    {
        private const string Id = nameof(ResolutionScopeAssemblyReferenceMustNotChange);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "Resolution scope assembly reference for '{0}' must not be changed.";
        private static new readonly string Category = Categories.Type;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly ResolutionScopeAssemblyReferenceMustNotChange Instance = new ();

        private ResolutionScopeAssemblyReferenceMustNotChange()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string assemblyName)
        {
            return new Message(Instance, assemblyName);
        }
    }
}
