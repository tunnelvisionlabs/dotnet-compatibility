namespace CompatibilityChecker.Descriptors
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// OtherError catch-all class
    /// </summary>
    internal class OtherError : CompatibilityDescriptor
    {
        private const string Id = nameof(OtherError);
        private static readonly string _title = TitleHelper.GenerateTitle(Id);
        private static readonly string _category = Categories.Type;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;

        private OtherError(string messageFormat = "Other unknown error")
            : base(Id, _title, messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string messageFormat)
        {
            
            return new Message(new OtherError(messageFormat));
        }
    }
}
