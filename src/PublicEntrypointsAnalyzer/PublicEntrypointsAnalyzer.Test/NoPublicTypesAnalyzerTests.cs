using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = NoPublicTypesAnalyzer.Test.CSharpCodeFixVerifier<
    PublicEntrypointsAnalyzer.NoPublicTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace NoPublicTypesAnalyzer.Test
{
    [TestClass]
    public class NoPublicTypesAnalyzerUnitTest
    {
        // No diagnostics expected to show up for an empty input
        [TestMethod]
        public async Task NoDiagnosticsForEmptyCode()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        // Diagnostic triggered for a public class
        [TestMethod]
        public async Task FlagsPublicClass()
        {
            var test = @"
    namespace TestNamespace
    {
        public class {|#0:PublicClass|}
        {
        }
    }";

            var expected = VerifyCS.Diagnostic("NPT0001")
                .WithLocation(0)
                .WithArguments("PublicClass");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // No diagnostics for allowed public entry point
        [TestMethod]
        public async Task NoDiagnosticsForAllowedPublicEntryPoint()
        {
            var test = @"
    namespace TestNamespace
    {
        public static class AllowedEntryPoint
        {
            public static void AllowedMethod(this string input)
            {
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        // Flags public types alongside the allowed entry point
        [TestMethod]
        public async Task FlagsPublicClassWithAllowedEntryPoint()
        {
            var test = @"
    namespace TestNamespace
    {
        public static class AllowedEntryPoint
        {
            public static void AllowedMethod(this string input)
            {
            }
        }

        public class {|#0:AnotherPublicClass|}
        {
        }
    }";

            var expected = VerifyCS.Diagnostic("NPT0001")
                .WithLocation(0)
                .WithArguments("AnotherPublicClass");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
