namespace CompatibilityCheckExample
{
    using System;
    using System.Reflection.PortableExecutable;
    using CompatibilityChecker;
    using NuGet;
    using Directory = System.IO.Directory;
    using File = System.IO.File;
    using Path = System.IO.Path;

    internal class Program
    {
        private static void Main(string[] args)
        {
            bool removeDirectory;
            string temporaryDirectory = GetTemporaryDirectory(out removeDirectory);
            Console.WriteLine("Working directory: {0}", temporaryDirectory);

            try
            {
                IPackageRepository sourceRepository = PackageRepositoryFactory.Default.CreateRepository("https://www.nuget.org/api/v2/");
                PackageManager packageManager = new PackageManager(sourceRepository, temporaryDirectory);
                packageManager.PackageInstalled += HandlePackageInstalled;
                packageManager.InstallPackage("Microsoft.Bcl.Immutable", SemanticVersion.Parse("1.0.34"));
                packageManager.InstallPackage("System.Collections.Immutable", SemanticVersion.Parse("1.1.33-beta"));

                using (PEReader referenceAssembly = new PEReader(File.OpenRead(Path.Combine(temporaryDirectory, "Microsoft.Bcl.Immutable.1.0.34", "lib", "portable-net45+win8+wp8+wpa81", "System.Collections.Immutable.dll"))))
                {
                    using (PEReader newAssembly = new PEReader(File.OpenRead(Path.Combine(temporaryDirectory, "System.Collections.Immutable.1.1.33-beta", "lib", "portable-net45+win8+wp8+wpa81", "System.Collections.Immutable.dll"))))
                    {
                        Analyzer analyzer = new Analyzer(referenceAssembly, newAssembly, null);
                        analyzer.Run();
                    }
                }
            }
            finally
            {
                if (removeDirectory)
                    Directory.Delete(temporaryDirectory, true);
            }
        }

        private static void HandlePackageInstalled(object sender, PackageOperationEventArgs e)
        {
            Console.WriteLine("Installed package: {0}", e.Package.GetFullName());
        }

        private static string GetTemporaryDirectory(out bool removeDirectory)
        {
            string packagesPath = Path.GetFullPath(@"..\..\..");
            if (Directory.Exists(Path.Combine(packagesPath, "packages")))
            {
                removeDirectory = false;
                return Path.Combine(packagesPath, "packages");
            }

            string path = Path.Combine(Path.GetTempPath(), "CompatibilityChecker-" + Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            removeDirectory = true;
            return path;
        }
    }
}
