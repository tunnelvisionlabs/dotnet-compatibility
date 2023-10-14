namespace CompatibilityChecker.Library
{
    using System;

    public class ConsoleMessageLogger : IMessageLogger
    {
        public virtual void Report(Message message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
