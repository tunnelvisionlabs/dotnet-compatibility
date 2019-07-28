using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using CompatibilityChecker;
using CommandLine;
using System.Collections.Generic;
using CommandLine.Text;

namespace CompatibilityCheckerCoreCLI
{
    using File = System.IO.File;
    using FileInfo = System.IO.FileInfo;

    internal class Program
    {
        public class Options
        {
            [Value(0, MetaName = "reference assembly", Required = true, HelpText = "The reference assembly.")]
            public string ReferenceAssembly { get; set; }

            [Value(1, MetaName = "new assembly", Required = true, HelpText = "The new assembly.")]
            public string NewAssembly { get; set; }

            [Option('a', "azure-pipelines", Required = false, Default = false, HelpText = "Include the logging prefixes for Azure Pipelines.")]
            public bool AzurePipelines { get; set; }

            [Usage()]
            public static IEnumerable<Example> Examples {
                get {
                    yield return new Example("Compare versions", new Options { ReferenceAssembly = "Assembly-1.0.0.dll", NewAssembly = "Assembly-1.0.1.dll" });
                    yield return new Example("Compare versions in Azure Pipelines as CI", new Options { ReferenceAssembly = "Assembly-1.0.0.dll", NewAssembly = "Assembly-1.0.1.dll", AzurePipelines = true });
                }
            }

        }

        private static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args)
               .WithParsed(opts => RunWithOptions(opts));
#if DEBUG
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
#endif
            if (result.Tag == ParserResultType.NotParsed)
            {
                Environment.ExitCode = 1;
            }
        }

        private static void RunWithOptions(Options opts)
        {
            Guid timeline_guid = Guid.NewGuid();
            FileInfo referenceFile = new FileInfo(opts.ReferenceAssembly);
            FileInfo newFile = new FileInfo(opts.NewAssembly);
            if (referenceFile.Exists && newFile.Exists)
            {
                if (opts.AzurePipelines)
                    Console.WriteLine("##vso[task.logdetail id={0};name=project1;type=build;order=1;state=Initialized]Starting...", timeline_guid);
                var refName = AssemblyName.GetAssemblyName(referenceFile.FullName);
                var newName = AssemblyName.GetAssemblyName(newFile.FullName);
                Console.WriteLine("{1}Using '{0}' as the reference assembly.", refName.FullName, opts.AzurePipelines ? "##vso[task.logdetail state=InProgress]" : string.Empty);
                Console.WriteLine("{1}Using '{0}' as the new assembly.", refName.FullName, opts.AzurePipelines ? "##vso[task.logdetail state=InProgress]" : string.Empty);
                using (PEReader referenceAssembly = new PEReader(File.OpenRead(referenceFile.FullName)))
                {
                    using (PEReader newAssembly = new PEReader(File.OpenRead(newFile.FullName)))
                    {
                        IMessageLogger logger = opts.AzurePipelines ? (IMessageLogger)new AzurePipelinesMessageLogger(timeline_guid) : new ConsoleMessageLogger();
                        Analyzer analyzer = new Analyzer(referenceAssembly, newAssembly, null, logger);
                        analyzer.Run();
                        if (analyzer.HasRun)
                        {
                            if (analyzer.ResultStatistics.SeverityCounts.error > 0)
                            {
                                Console.WriteLine(string.Format("{3}Analyzer done. {0} errors, {1} warnings, {2} informational items.", analyzer.ResultStatistics.SeverityCounts.error, analyzer.ResultStatistics.SeverityCounts.warning, analyzer.ResultStatistics.SeverityCounts.information, opts.AzurePipelines ? "##vso[task.complete result=SucceededWithIssues]" : string.Empty));
                                Environment.ExitCode = -2;
                                return;
                            }
                            else
                            {
                                Console.WriteLine(string.Format("{3}Analyzer done. {0} errors, {1} warnings, {2} informational items.", analyzer.ResultStatistics.SeverityCounts.error, analyzer.ResultStatistics.SeverityCounts.warning, analyzer.ResultStatistics.SeverityCounts.information, opts.AzurePipelines ? "##vso[task.complete result=Succeeded]" : string.Empty));
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format("{0}Analyzer failed to run.", opts.AzurePipelines ? "##vso[task.complete result=Failed]" : string.Empty));

                            Environment.ExitCode = -1;
                            return;
                        }
                    }
                }

            }
            else
            {
                if (!referenceFile.Exists)
                    Console.Error.WriteLine("{1}Reference file '{0}' not found or inaccessible.", referenceFile.FullName, opts.AzurePipelines ? "##vso[task.logissue type=error]" : string.Empty);
                if (!newFile.Exists)
                    Console.Error.WriteLine("{1}New file '{0}' not found or inaccessible.", newFile.FullName, opts.AzurePipelines ? "##vso[task.logissue type=error]" : string.Empty);
                Environment.ExitCode = 2;
                return;
            }
        }
    }
}