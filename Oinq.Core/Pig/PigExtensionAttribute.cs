using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping binder method names to Pig extensions.
    /// </summary>
    public sealed class PigExtension : Attribute
    {
        // private fields
        private String _name;

        // constructors
        /// <summary>
        /// Initializes an instance of PigExtension.
        /// </summary>
        /// <param path="binderName">Name of the binder class.</param>
        public PigExtension(String binderName)
        {
            _name = binderName;
        }

        // public properties
        /// <summary>
        /// Gets the path of the method in the extension.
        /// </summary>
        public String BinderName
        {
            get { return _name; }
        }
    }
}
