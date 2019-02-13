using System.Collections.Generic;

namespace CompatibilityChecker
{
    public interface IMessageReporter
    {
        IEnumerable<Message> ReportedMessages { get; }

        IReportStatistics Statistics { get; }

        void Report(Message message);
        
    }
}
