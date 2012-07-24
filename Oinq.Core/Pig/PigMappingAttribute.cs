using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping field names to Pig query field names.
    /// </summary>
    public sealed class PigMapping : Attribute
    {
        // private fields
        private String _name;

        // constructors
        /// <summary>
        /// Initializes an member of PigMapping.
        /// </summary>
        /// <param path="path"></param>
        public PigMapping(String name)
        {
            _name = name;
        }

        // public properties
        /// <summary>
        /// Gets the path of the field in the data source.
        /// </summary>
        public String Name
        {
            get { return _name; }
        }
    }
}
