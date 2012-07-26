using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping field names to Pig query field names.
    /// </summary>
    public sealed class PigMapping : Attribute
    {
        // constructors
        /// <summary>
        /// Initializes an member of PigMapping.
        /// </summary>
        /// <param name="name">Mapping name</param>
        public PigMapping(String name)
        {
            Name = name;
        }

        // public properties
        /// <summary>
        /// Gets the path of the field in the data source.
        /// </summary>
        public string Name { get; private set; }
    }
}
