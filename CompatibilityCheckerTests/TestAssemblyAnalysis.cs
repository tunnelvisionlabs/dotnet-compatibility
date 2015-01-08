namespace CompatibilityCheckerTests
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Reflection.Emit;
    using CompatibilityChecker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestAssemblyAnalysis
    {
        [TestMethod]
        public void TestAssemblyNameMustNotBeChanged_Pass()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.ReflectionOnly);
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.ReflectionOnly);
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAssemblyNameMustNotBeChanged_Fail()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.ReflectionOnly);
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly.V2");
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.ReflectionOnly);
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error AssemblyNameMustNotBeChanged: The simple name of an assembly cannot change.", messages[0].ToString());
        }
    }
}
