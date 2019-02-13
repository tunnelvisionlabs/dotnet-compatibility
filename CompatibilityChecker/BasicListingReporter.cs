namespace CompatibilityChecker
{
    using System;
    using System.Collections.Generic;

    internal class BasicListingReporter : IMessageReporter
    {
        IMessageLogger _logger;

        List<Message> _reportedMessages;

        int _severityError = 0;
        int _severityWarning = 0;
        int _severityInformation = 0;
        int _severityDisabled = 0;

        IEnumerable<Message> IMessageReporter.ReportedMessages => _reportedMessages;

        IReportStatistics IMessageReporter.Statistics => new BasicListingStatistics(_severityError, _severityWarning, _severityInformation, _severityDisabled);

        public BasicListingReporter(IMessageLogger logger)
        {
            _logger = logger;

            _reportedMessages = new List<Message>();
        }
        
        void IMessageReporter.Report(Message message)
        {
            switch (message.Severity)
            {
                case Severity.Error:
                    _severityError++;
                    break;
                case Severity.Warning:
                    _severityWarning++;
                    break;
                case Severity.Information:
                    _severityInformation++;
                    break;
                case Severity.Disabled:
                    _severityDisabled++;
                    break;
                default:
                    throw new ArgumentException(string.Format("Severity {0} is not supported by this Message Reporter.", message.Severity));
            }
            _reportedMessages.Add(message);
            _logger.Report(message);
        }
    }
}
