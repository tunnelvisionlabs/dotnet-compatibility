namespace CompatibilityChecker.Library.Descriptors
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// OtherError catch-all class
    /// </summary>
    internal class OtherError : CompatibilityDescriptor
    {
        private const string Id = nameof(OtherError);
        private static new readonly string Title = TitleHelper.GenerateTitle(Id);

        private static new readonly string MessageFormat = "Other error occured. {0}";
        private static new readonly string Category = Categories.Other;
        private static new readonly Severity DefaultSeverity = Severity.Error;
        private static new readonly string Description = null;

        private static readonly OtherError Instance = new ();

        private OtherError()
            : base(Id, Title, MessageFormat, Category, DefaultSeverity, Description)
        {
        }

        internal static Message CreateMessage(string error)
        {
            return new Message(Instance, error);
        }
    }
}
