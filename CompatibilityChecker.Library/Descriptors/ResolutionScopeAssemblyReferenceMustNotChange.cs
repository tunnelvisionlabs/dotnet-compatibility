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
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "Resolution scope assembly reference for '{0}' must not be changed.";
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly ResolutionScopeAssemblyReferenceMustNotChange Instance = new ResolutionScopeAssemblyReferenceMustNotChange();

        private ResolutionScopeAssemblyReferenceMustNotChange()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string assemblyName)
        {
            return new Message(Instance, assemblyName);
        }
    }
}
