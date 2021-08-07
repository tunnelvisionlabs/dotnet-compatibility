using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;

namespace CompatibilityChecker.Library.Tests
{
    using File = System.IO.File;

    internal static class TestUtility
    {
        public static ReadOnlyCollection<Message> AnalyzeAssemblies(string referenceAssemblyFile, string newAssemblyFile)
        {
            using PEReader referenceAssembly = new (File.OpenRead(referenceAssemblyFile));
            using PEReader newAssembly = new (File.OpenRead(newAssemblyFile));

            return AnalyzeAssemblies(referenceAssembly, newAssembly);
        }

        public static ReadOnlyCollection<Message> AnalyzeAssemblies(ImmutableArray<byte> referenceAssemblyBytes, ImmutableArray<byte> newAssemblyBytes)
        {
            using PEReader referenceAssembly = new (referenceAssemblyBytes);
            using PEReader newAssembly = new (newAssemblyBytes);

            return AnalyzeAssemblies(referenceAssembly, newAssembly);
        }

        public static ReadOnlyCollection<Message> AnalyzeAssemblies(PEReader referenceAssembly, PEReader newAssembly)
        {
            TestMessageLogger logger = new ();
            BasicListingReporter reporter = new (logger);
            Analyzer analyzer = new (referenceAssembly, newAssembly, reporter, logger);
            analyzer.Run();

            return logger.RawMessages;
        }

        private class TestMessageLogger : IMessageLogger
        {
            private readonly List<Message> rawMessages = new ();
            private readonly List<string> messages = new ();

            public ReadOnlyCollection<Message> RawMessages => rawMessages.AsReadOnly();

            public ReadOnlyCollection<string> Messages => messages.AsReadOnly();

            public void Report(Message message)
            {
                rawMessages.Add(message);
                messages.Add(message.ToString());
            }
        }
    }
}
