namespace CompatibilityChecker
{
    using System;

    public class AzurePipelinesMessageLogger : IMessageLogger
    {
        public virtual void Report(Message message)
        {
            switch (message.Severity)
            {
                case Severity.Error:
                    Console.Error.WriteLine("##vso[task.logissue type=error]{0}", message);
                    break;
                case Severity.Warning:
                    Console.Error.WriteLine("##vso[task.logissue type=warning]{0}", message);
                    break;
                default:
                    Console.WriteLine("##vso[task.logdetail]{0}",message);
                    break;
            }
                
        }
    }
}
