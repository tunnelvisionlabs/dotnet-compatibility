namespace CompatibilityCheckerTests
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Reflection.Emit;
    using CompatibilityChecker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestTypeAnalysis
    {
        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_PassUnchanged()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_PassRemoved()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_PassNotPublic()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Abstract, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_Fail()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error AbstractMustNotBeAddedToType: The 'abstract' modifier cannot be added to a type.", messages[0].ToString());
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_PassUnchanged()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_PassRemoved()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_PassNotPublic()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_Fail()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.dll");
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            referenceAssemblyBuilder.Save("Test.Assembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name, "Test.Assembly.V2.dll");
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            newAssemblyBuilder.Save("Test.Assembly.V2.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error SealedMustNotBeAddedToType: The 'sealed' modifier cannot be added to a type.", messages[0].ToString());
        }
    }
}
