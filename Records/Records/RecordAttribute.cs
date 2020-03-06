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
        public RecordAttribute(bool with = false)
        {
            With = with;
        }

        public bool With { get; }
    }
}
