using System;
using Microsoft.CodeAnalysis;

namespace Records
{
    internal static class Extensions
    {
        public static void ReportError(this IProgress<Diagnostic> diagnostics, string message, string codeGenCategory, SyntaxNode node)
        {
            diagnostics.Report(message, codeGenCategory, DiagnosticSeverity.Error, node);
        }

        private static void Report(this IProgress<Diagnostic> diagnostics, string message, string codeGenCategory, DiagnosticSeverity severity, SyntaxNode node)
        {
            diagnostics.Report(Diagnostic.Create(
                new DiagnosticDescriptor(
                    $"CG{Math.Abs(message.GetHashCode() >> 16)}",
                    message,
                    message,
                    codeGenCategory,
                    severity,
                    true,
                    message),
                node.GetLocation()));
        }
    }
}
