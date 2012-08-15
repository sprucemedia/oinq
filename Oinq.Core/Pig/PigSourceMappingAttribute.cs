using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping data sources to Pig query source names.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PigSourceMapping : Attribute
    {
        private readonly String _path;

        public PigSourceMapping(String path)
        {
            _path = path;
        }

        public string Path { get { return _path; } }

        public Int32 Order { get; set; }
    }
}