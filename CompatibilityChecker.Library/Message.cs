namespace CompatibilityChecker.Library
{
    using CompatibilityChecker.Library.Descriptors;

    public class Message
    {
        private readonly CompatibilityDescriptor descriptor;
        private readonly object[] arguments;

        internal Message(CompatibilityDescriptor descriptor, params object[] arguments)
        {
            this.descriptor = descriptor;
            this.arguments = arguments;
        }

        internal Severity Severity => descriptor.DefaultSeverity;

        public override string ToString()
        {
            string message = string.Format(descriptor.MessageFormat, arguments);
            return string.Format("{0} {1}: {2}", descriptor.DefaultSeverity, descriptor.RuleId, message);
        }
    }
}
