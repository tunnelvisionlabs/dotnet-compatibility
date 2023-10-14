namespace CompatibilityChecker.Library.Descriptors
{
    using System.Reflection;

    /// <summary>
    /// The public key of a strong-named assembly cannot be changed.
    /// </summary>
    /// <remarks>
    /// <para>The public key of an assembly corresponds to the value returned by the
    /// <see cref="AssemblyName.GetPublicKey"/> method.</para>
    /// <para>Adding a strong name to an assembly that did not previously have a strong name is not considered a
    /// breaking change.</para>
    /// </remarks>
    internal class PublicKeyMustNotBeChanged : CompatibilityDescriptor
    {
        private const string Id = nameof(PublicKeyMustNotBeChanged);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);
        private static new readonly string MessageFormat = "The public key of a strong-named assembly '{0}' cannot change.";
        private static new readonly string Category = Categories.Assembly;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly PublicKeyMustNotBeChanged Instance = new ();

        private PublicKeyMustNotBeChanged()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string assemblyName)
        {
            return new Message(Instance, assemblyName);
        }
    }
}
