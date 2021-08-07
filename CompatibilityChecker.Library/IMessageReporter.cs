using System.Collections.Generic;

namespace CompatibilityChecker.Library
{
    public interface IMessageReporter
    {
        IEnumerable<Message> ReportedMessages { get; }

        IReportStatistics Statistics { get; }

        void Report(Message message);
    }
}
