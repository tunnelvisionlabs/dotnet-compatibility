using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.PortableExecutable;
using CommandLine;
using CommandLine.Text;
using CompatibilityChecker.Library;

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

            [Option('w', "warnings-only", Required = false, Default = false, HelpText = "Do not raise errors for Azure Pipelines, it also swallows the return code.")]
            public bool WarningsOnly { get; set; }

            [Usage(ApplicationAlias = "dotnet compat")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Compare versions", new Options { ReferenceAssembly = "Assembly-1.0.0.dll", NewAssembly = "Assembly-1.0.1.dll" });
                    yield return new Example("Compare versions in Azure Pipelines as CI", new Options { ReferenceAssembly = "Assembly-1.0.0.dll", NewAssembly = "Assembly-1.0.1.dll", AzurePipelines = true });
                    yield return new Example("Compare versions in Azure Pipelines as CI without failing the CI job", new Options { ReferenceAssembly = "Assembly-1.0.0.dll", NewAssembly = "Assembly-1.0.1.dll", AzurePipelines = true, WarningsOnly = true });
                }
            }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Running dotnet-compat v{0}...", Assembly.GetExecutingAssembly().GetName().Version);
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
            //Guid timeline_guid = Guid.NewGuid();
            FileInfo referenceFile = new(opts.ReferenceAssembly);
            FileInfo newFile = new(opts.NewAssembly);
            if (referenceFile.Exists && newFile.Exists)
            {
                //if (opts.AzurePipelines)
                //Console.WriteLine("##vso[task.logdetail id={0};name=BinaryCompatibilityCheck;type=build;order=1;state=Initialized]Starting...", timeline_guid);
                var refName = AssemblyName.GetAssemblyName(referenceFile.FullName);
                var newName = AssemblyName.GetAssemblyName(newFile.FullName);
                Console.WriteLine("Using '{0}' as the reference assembly.", refName.FullName);
                Console.WriteLine("Using '{0}' as the new assembly.", refName.FullName);
                using (PEReader referenceAssembly = new(File.OpenRead(referenceFile.FullName)))
                {
                    using (PEReader newAssembly = new(File.OpenRead(newFile.FullName)))
                    {
                        IMessageLogger logger;
                        if (opts.AzurePipelines)
                        {
                            if (opts.WarningsOnly)
                            {
                                logger = new AzurePipelinesMessageLogger(Severity.Warning);
                            }
                            else
                            {
                                logger = new AzurePipelinesMessageLogger();
                            }
                        }
                        else
                        {
                            logger = new ConsoleMessageLogger();
                        }

                        Analyzer analyzer = new(referenceAssembly, newAssembly, null, logger);
                        analyzer.Run();
                        if (analyzer.HasRun)
                        {
                            Console.WriteLine(string.Format("Analyzer done. {0} errors, {1} warnings, {2} informational items.", analyzer.ResultStatistics.SeverityCounts.error, analyzer.ResultStatistics.SeverityCounts.warning, analyzer.ResultStatistics.SeverityCounts.information));

                            if (analyzer.ResultStatistics.SeverityCounts.error > 0)
                            {
                                if (opts.AzurePipelines)
                                {
                                    Console.WriteLine("##vso[task.complete result=SucceededWithIssues]");
                                }

                                Environment.ExitCode = opts.WarningsOnly ? 0 : -2;
                                return;
                            }
                            else
                            {
                                if (opts.AzurePipelines)
                                {
                                    Console.WriteLine("##vso[task.complete result=Succeeded]");
                                }

                                return;
                            }
                        }
                        else
                        {
                            if (opts.AzurePipelines)
                            {
                                Console.WriteLine("##vso[task.complete result=Failed]");
                            }

                            Environment.ExitCode = opts.WarningsOnly ? 0 : -1;
                            return;
                        }
                    }
                }
            }
            else
            {
                if (!referenceFile.Exists)
                {
                    Console.Error.WriteLine("{1}Reference file '{0}' not found or inaccessible.", referenceFile.FullName, opts.AzurePipelines ? "##vso[task.logissue type=error]" : string.Empty);
                }

                if (!newFile.Exists)
                {
                    Console.Error.WriteLine("{1}New file '{0}' not found or inaccessible.", newFile.FullName, opts.AzurePipelines ? "##vso[task.logissue type=error]" : string.Empty);
                }

                Environment.ExitCode = 2;
                return;
            }
        }
    }
}
