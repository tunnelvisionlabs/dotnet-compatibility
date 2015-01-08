namespace CompatibilityCheckerTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection.PortableExecutable;
    using CompatibilityChecker;
    using File = System.IO.File;

    internal static class TestUtility
    {
        public static ReadOnlyCollection<Message> AnalyzeAssemblies(string referenceAssemblyFile, string newAssemblyFile)
        {
            using (PEReader referenceAssembly = new PEReader(File.OpenRead(referenceAssemblyFile)))
            {
                using (PEReader newAssembly = new PEReader(File.OpenRead(newAssemblyFile)))
                {
                    TestMessageLogger logger = new TestMessageLogger();
                    Analyzer analyzer = new Analyzer(referenceAssembly, newAssembly, logger);
                    analyzer.Run();

                    return logger.RawMessages;
                }
            }
        }

        internal class TestMessageLogger : IMessageLogger
        {
            private readonly List<Message> _rawMessages = new List<Message>();
            private readonly List<string> _messages = new List<string>();

            public ReadOnlyCollection<Message> RawMessages
            {
                get
                {
                    return _rawMessages.AsReadOnly();
                }
            }

            public ReadOnlyCollection<string> Messages
            {
                get
                {
                    return _messages.AsReadOnly();
                }
            }

            public void Report(Message message)
            {
                _rawMessages.Add(message);
                _messages.Add(message.ToString());
            }
        }
    }
}
