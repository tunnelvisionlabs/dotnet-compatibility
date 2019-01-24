namespace CompatibilityChecker.Descriptors
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
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "The simple name of an assembly cannot change.";
        private static readonly string _category = Categories.Assembly;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly AssemblyNameMustNotBeChanged Instance = new AssemblyNameMustNotBeChanged();

        private AssemblyNameMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage()
        {
            return new Message(Instance);
        }
    }
}
