using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Lokad.ILPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompatibilityChecker.Library.Tests
{
    [TestClass]
    public class TestTypeAnalysis
    {
        private readonly AssemblyGenerator _generator = new();

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_PassUnchanged()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_PassRemoved()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_PassNotPublic()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Abstract, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestAbstractMustNotBeAddedToType_Fail()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error AbstractMustNotBeAddedToType: The 'abstract' modifier cannot be added to type '.MyType'.", messages[0].ToString());
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_PassUnchanged()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_PassRemoved()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_PassNotPublic()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void TestSealedMustNotBeAddedToType_Fail()
        {
            AssemblyName assemblyName = new AssemblyName("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            _generator.GenerateAssembly(referenceAssemblyBuilder, "Test.Assembly.dll");

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            _generator.GenerateAssembly(newAssemblyBuilder, "Test.Assembly.V2.dll");

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies("Test.Assembly.dll", "Test.Assembly.V2.dll");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Error SealedMustNotBeAddedToType: The 'sealed' modifier cannot be added to type '.MyType'.", messages[0].ToString());
        }
    }
}
