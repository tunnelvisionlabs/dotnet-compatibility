using System;
using System.Reflection.PortableExecutable;
using CompatibilityChecker;

namespace CompatibilityCheckerCoreCLI
{
    using File = System.IO.File;
    using FileInfo = System.IO.FileInfo;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: reference.dll new.dll");
                Environment.ExitCode = 1;
            }
            else
            {

                FileInfo referenceFile = new FileInfo(args[0]);
                FileInfo newFile = new FileInfo(args[1]);
                if (referenceFile.Exists && newFile.Exists)
                {
                    using (PEReader referenceAssembly = new PEReader(File.OpenRead(referenceFile.FullName)))
                    {
                        using (PEReader newAssembly = new PEReader(File.OpenRead(newFile.FullName)))
                        {
                            Analyzer analyzer = new Analyzer(referenceAssembly, newAssembly, null, null);
                            analyzer.Run();
                            if (analyzer.HasRun)
                            {
                                Console.Error.WriteLine(string.Format("Analyzer done. {0} errors, {1} warnings, {2} informational items.", analyzer.ResultStatistics.SeverityCounts.error, analyzer.ResultStatistics.SeverityCounts.warning, analyzer.ResultStatistics.SeverityCounts.information));
                                if (analyzer.ResultStatistics.SeverityCounts.error > 0)
                                {
                                    Environment.ExitCode = -2;
                                }
                            }
                            else
                            {
                                Environment.ExitCode = -1;
                            }
                        }
                    }
                }
            }
        }
    }
}