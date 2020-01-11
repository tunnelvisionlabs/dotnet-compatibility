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

        private static readonly string _messageFormat = "Other error occured. {0}";
        private static readonly string _category = Categories.Other;
        private static readonly Severity _defaultSeverity = Severity.Error;
        private static readonly string _description = null;


        private static readonly OtherError Instance = new OtherError();

        private OtherError()
            : base(Id, _title, _messageFormat, _category, _defaultSeverity, _description)
        {
        }

        internal static Message CreateMessage(string error)
        {
            return new Message(Instance, error);
        }
    }
}
