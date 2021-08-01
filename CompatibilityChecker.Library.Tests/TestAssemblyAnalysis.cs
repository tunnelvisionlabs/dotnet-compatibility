using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Lokad.ILPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompatibilityChecker.Library.Tests
{
    [TestClass]
    public class TestAssemblyAnalysis
    {
        private readonly AssemblyGenerator _generator = new();

        [TestMethod]
        public void TestAssemblyNameMustNotBeChanged_Pass()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.RunAndCollect);
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder draftAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.RunAndCollect);
            _generator.GenerateAssembly(draftAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAssemblyNameMustNotBeChanged_Fail()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssembly = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.Run);
            referenceAssembly.DefineDynamicModule("TestAssembly");
            _generator.GenerateAssembly(referenceAssembly, "Test.Assembly.dll");

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly.V2");
            AssemblyBuilder draftAssembly = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.Run);
            draftAssembly.DefineDynamicModule("TestAssemblyV2");
            _generator.GenerateAssembly(draftAssembly, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error AssemblyNameMustNotBeChanged: The simple name of an assembly cannot change for 'Test.Assembly'.", messages[0].ToString());
        }
    }
}
