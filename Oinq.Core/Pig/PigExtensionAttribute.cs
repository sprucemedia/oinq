using System;

namespace Oinq
{
    /// <summary>
    /// Attribute used for mapping binder method names to Pig extensions.
    /// </summary>
    public sealed class PigExtensionAttribute : Attribute
    {
        // private fields
        private readonly Type _binderType;

        // constructors
        /// <summary>
        /// Initializes an member of PigExtension.
        /// </summary>
        /// <param name="binderType">Type of the binder class.</param>
        public PigExtensionAttribute(Type binderType)
        {
            _binderType = binderType;
        }

        // public properties
        /// <summary>
        /// Gets the type of the class that contains the binder.
        /// </summary>
        public Type BinderType
        {
            get { return _binderType; }
        }
    }
}