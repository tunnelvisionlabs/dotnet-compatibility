namespace CompatibilityCheckerTests
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Reflection.Emit;
    using CompatibilityChecker.Library;
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
            Assert.AreEqual("Error AssemblyNameMustNotBeChanged: The simple name of an assembly cannot change for 'Test.Assembly'.", messages[0].ToString());
        }

        [TestMethod]
        public void TestPublicKeyMustNotBeChanged_PassMissing()
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
        public void TestPublicKeyMustNotBeChanged_PassUnchanged()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            referenceName.KeyPair = TestUtility.GenerateStrongNameKeyPair();
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.ReflectionOnly);
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly");
            newAssemblyName.KeyPair = referenceName.KeyPair;
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.ReflectionOnly);
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestPublicKeyMustNotBeChanged_PassAdded()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.ReflectionOnly);
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly");
            newAssemblyName.KeyPair = TestUtility.GenerateStrongNameKeyPair();
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.ReflectionOnly);
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestPublicKeyMustNotBeChanged_Fail()
        {
            AssemblyName referenceName = new AssemblyName("Test.Assembly");
            referenceName.KeyPair = TestUtility.GenerateStrongNameKeyPair();
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.ReflectionOnly);
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyName newAssemblyName = new AssemblyName("Test.Assembly");
            newAssemblyName.KeyPair = TestUtility.GenerateStrongNameKeyPair();
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.ReflectionOnly);
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error PublicKeyMustNotBeChanged: The public key of a strong-named assembly 'Test.Assembly' cannot change.", messages[0].ToString());
        }
    }
}
