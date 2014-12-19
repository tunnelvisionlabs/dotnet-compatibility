namespace CompatibilityChecker
{
    using System;

    internal class ConsoleMessageLogger : IMessageLogger
    {
        public virtual void Report(Message message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
