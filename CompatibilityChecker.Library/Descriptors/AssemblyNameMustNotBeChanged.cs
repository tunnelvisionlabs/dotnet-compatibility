namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection;

    /// <summary>
    /// The simple name of an assembly cannot be changed.
    /// </summary>
    /// <remarks>
    /// <para>The simple name of an assembly corresponds to the value of the <see cref="AssemblyName.Name"/>
    /// property.</para>
    /// </remarks>
    internal class AssemblyNameMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(AssemblyNameMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "The simple name of an assembly cannot change for '{0}'.";
        private static new readonly string Category = Categories.Assembly;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly AssemblyNameMustNotBeChanged Instance = new ();

        private AssemblyNameMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string assemblyName)
        {
            return new Message(Instance, assemblyName);
        }
    }
}
