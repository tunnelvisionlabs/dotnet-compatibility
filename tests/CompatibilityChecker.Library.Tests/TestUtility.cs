using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;

namespace CompatibilityChecker.Library.Tests
{
    using File = System.IO.File;

    internal static class TestUtility
    {
        public static ReadOnlyCollection<Message> AnalyzeAssemblies(string referenceAssemblyFile, string newAssemblyFile)
        {
            using PEReader referenceAssembly = new PEReader(File.OpenRead(referenceAssemblyFile));
            using PEReader newAssembly = new PEReader(File.OpenRead(newAssemblyFile));

            TestMessageLogger logger = new TestMessageLogger();
            BasicListingReporter reporter = new BasicListingReporter(logger);
            Analyzer analyzer = new Analyzer(referenceAssembly, newAssembly, reporter, logger);
            analyzer.Run();

            return logger.RawMessages;
        }

        private class TestMessageLogger : IMessageLogger
        {
            private readonly List<Message> _rawMessages = new();
            private readonly List<string> _messages = new();

            public ReadOnlyCollection<Message> RawMessages => _rawMessages.AsReadOnly();

            public ReadOnlyCollection<string> Messages => _messages.AsReadOnly();

            public void Report(Message message)
            {
                _rawMessages.Add(message);
                _messages.Add(message.ToString());
            }
        }
    }
}
