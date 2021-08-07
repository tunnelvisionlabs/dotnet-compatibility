using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Lokad.ILPack;
using Xunit;

namespace CompatibilityChecker.Library.Tests
{
    public class TestTypeAnalysis
    {
        private readonly AssemblyGenerator generator = new ();

        [Fact]
        public void TestAbstractMustNotBeAddedToType_PassUnchanged()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestAbstractMustNotBeAddedToType_PassRemoved()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestAbstractMustNotBeAddedToType_PassNotPublic()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Abstract, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestAbstractMustNotBeAddedToType_Fail()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Abstract, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Family);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Single(messages);
            Assert.Equal("Error AbstractMustNotBeAddedToType: The 'abstract' modifier cannot be added to type '.MyType'.", messages[0].ToString());
        }

        [Fact]
        public void TestSealedMustNotBeAddedToType_PassUnchanged()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestSealedMustNotBeAddedToType_PassRemoved()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestSealedMustNotBeAddedToType_PassNotPublic()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestSealedMustNotBeAddedToType_Fail()
        {
            AssemblyName assemblyName = new ("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder referenceModuleBuilder = referenceAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder referenceTypeBuilder = referenceModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public, typeof(object));
            referenceTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            referenceTypeBuilder.CreateType();
            var v1Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModuleBuilder = newAssemblyBuilder.DefineDynamicModule(assemblyName.Name!);
            TypeBuilder newTypeBuilder = newModuleBuilder.DefineType("MyType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
            newTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            newTypeBuilder.CreateType();
            var v2Bytes = ImmutableArray.Create(generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Single(messages);
            Assert.Equal("Error SealedMustNotBeAddedToType: The 'sealed' modifier cannot be added to type '.MyType'.", messages[0].ToString());
        }
    }
}
