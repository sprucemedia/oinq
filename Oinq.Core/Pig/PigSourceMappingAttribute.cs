using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping data sources to Pig query source names.
    /// </summary>
    public sealed class PigSourceMapping : Attribute
    {
        // private fields
        private String _path;

        // constructors
        /// <summary>
        /// Initializes an instance of PigMapping.
        /// </summary>
        /// <param path="path"></param>
        public PigSourceMapping(String path)
        {
            _path = path;
        }

        // public properties
        /// <summary>
        /// Gets the path to the data source.
        /// </summary>
        public String Path
        {
            get { return _path; }
        }
    }
}
