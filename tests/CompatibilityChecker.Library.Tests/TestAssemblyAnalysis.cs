using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Lokad.ILPack;
using Xunit;

namespace CompatibilityChecker.Library.Tests
{
    public class TestAssemblyAnalysis
    {
        private readonly AssemblyGenerator _generator = new();

        [Fact]
        public void TestAssemblyNameMustNotBeChanged_Pass()
        {
            AssemblyName referenceName = new("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.RunAndCollect);
            var v1Bytes = ImmutableArray.Create(_generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyName newAssemblyName = new("Test.Assembly");
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.RunAndCollect);
            var v2Bytes = ImmutableArray.Create(_generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Empty(messages);
        }

        [Fact]
        public void TestAssemblyNameMustNotBeChanged_Fail()
        {
            AssemblyName referenceName = new("Test.Assembly");
            AssemblyBuilder referenceAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(referenceName, AssemblyBuilderAccess.Run);
            referenceAssemblyBuilder.DefineDynamicModule("TestAssembly");
            var v1Bytes = ImmutableArray.Create(_generator.GenerateAssemblyBytes(referenceAssemblyBuilder));

            AssemblyName newAssemblyName = new("Test.Assembly.V2");
            AssemblyBuilder newAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(newAssemblyName, AssemblyBuilderAccess.Run);
            newAssemblyBuilder.DefineDynamicModule("TestAssemblyV2");
            var v2Bytes = ImmutableArray.Create(_generator.GenerateAssemblyBytes(newAssemblyBuilder));

            ReadOnlyCollection<Message> messages = TestUtility.AnalyzeAssemblies(v1Bytes, v2Bytes);
            Assert.Single(messages);
            Assert.Equal("Error AssemblyNameMustNotBeChanged: The simple name of an assembly cannot change for 'Test.Assembly'.", messages[0].ToString());
        }
    }
}
