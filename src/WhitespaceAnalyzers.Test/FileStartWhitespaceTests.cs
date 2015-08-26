using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace WhitespaceAnalyzers.Test
{
    [TestClass]
    public class FileStartWhitespaceTests : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void WS001_Pass()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void WS001_Violation()
        {
            var textNewline = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "WS001",
                Message = "File starts with whitespace",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 0, 0)
                        }
            };

            VerifyCSharpDiagnostic(textNewline, expected);

            var textSpace = @" using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}
";

            VerifyCSharpDiagnostic(textSpace, expected);

            var fixtest = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}
";
            VerifyCSharpFix(textNewline, fixtest);
            VerifyCSharpFix(textSpace, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new FileStartWhitespaceCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FileStartWhitespaceAnalyzer();
        }
    }
}