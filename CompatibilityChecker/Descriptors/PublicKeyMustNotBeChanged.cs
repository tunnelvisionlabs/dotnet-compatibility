namespace CompatibilityChecker.Descriptors
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
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _messageFormat = "The public key of a strong-named assembly cannot change.";
        private static readonly string _category = Categories.Assembly;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private static readonly PublicKeyMustNotBeChanged Instance = new PublicKeyMustNotBeChanged();

        private PublicKeyMustNotBeChanged()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage()
        {
            return new Message(Instance);
        }
    }
}
