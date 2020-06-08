namespace CompatibilityChecker.Library
{
    using CompatibilityChecker.Library.Descriptors;

    public class Message
    {
        private readonly CompatibilityDescriptor _descriptor;
        private readonly object[] _arguments;

        internal Message(CompatibilityDescriptor descriptor, params object[] arguments)
        {
            _descriptor = descriptor;
            _arguments = arguments;
        }

        internal Severity Severity => _descriptor.DefaultSeverity;

        public override string ToString()
        {
            string message = string.Format(_descriptor.MessageFormat, _arguments);
            return string.Format("{0} {1}: {2}", _descriptor.DefaultSeverity, _descriptor.RuleId, message);
        }
    }
}
