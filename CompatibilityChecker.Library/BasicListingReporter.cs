namespace CompatibilityChecker.Library
{
    using System;
    using System.Collections.Generic;

    internal class BasicListingReporter : IMessageReporter
    {
        private IMessageLogger logger;

        private List<Message> reportedMessages;

        private int severityError = 0;
        private int severityWarning = 0;
        private int severityInformation = 0;
        private int severityDisabled = 0;

        IEnumerable<Message> IMessageReporter.ReportedMessages => reportedMessages;

        IReportStatistics IMessageReporter.Statistics => new BasicListingStatistics(severityError, severityWarning, severityInformation, severityDisabled);

        public BasicListingReporter(IMessageLogger logger)
        {
            this.logger = logger;

            reportedMessages = new List<Message>();
        }

        void IMessageReporter.Report(Message message)
        {
            switch (message.Severity)
            {
                case Severity.Error:
                    severityError++;
                    break;
                case Severity.Warning:
                    severityWarning++;
                    break;
                case Severity.Information:
                    severityInformation++;
                    break;
                case Severity.Disabled:
                    severityDisabled++;
                    break;
                default:
                    throw new ArgumentException(string.Format("Severity {0} is not supported by this Message Reporter.", message.Severity));
            }

            reportedMessages.Add(message);
            logger.Report(message);
        }
    }
}
