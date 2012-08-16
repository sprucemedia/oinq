using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping data sources to Pig query source names.
    /// </summary>
    public sealed class PigSourceMapping : Attribute
    {
        // constructors
        /// <summary>
        /// Initializes an member of PigMapping.
        /// </summary>
        /// <param path="path"></param>
        public PigSourceMapping(String path)
        {
            Path = path;
        }

        // public properties
        /// <summary>
        /// Gets the path to the data source.
        /// </summary>
        public string Path { get; private set; }
    }
}