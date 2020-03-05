using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace Records
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    [CodeGenerationAttribute(typeof(RecordCodeGenerator))]
    public sealed class RecordAttribute : Attribute
    {
    }
}
