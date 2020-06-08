namespace CompatibilityChecker.Library
{
    using System;

    public class AzurePipelinesMessageLogger : IMessageLogger
    {
        Severity? maxSeverity;

        public AzurePipelinesMessageLogger(Severity? MaxSeverity)
        {
            maxSeverity = MaxSeverity;
        }

        public AzurePipelinesMessageLogger()
        {
            maxSeverity = null;
        }

        public virtual void Report(Message message)
        {
            Severity sev;
            if (maxSeverity.HasValue)
            {
                sev = message.Severity > maxSeverity.Value ? maxSeverity.Value : message.Severity;
            }
            else
            {
                sev = message.Severity;
            }
            switch (sev)
            {
                case Severity.Error:
                    Console.Error.WriteLine("##vso[task.logissue type=error]{0}", message);
                    break;
                case Severity.Warning:
                    Console.Error.WriteLine("##vso[task.logissue type=warning]{0}", message);
                    break;
                default:
                    Console.WriteLine(message);
                    break;
            }
                
        }
    }
}
