using System;

namespace Oinq.EdgeSpring.Entity
{
    /// <summary>
    /// Attribute used for defining property types to EdgeSpring entities.
    /// </summary>
    public sealed class EntityPropertyAttribute : Attribute
    {
        // private fields
        private readonly EntityPropertyType _propertyType;

        // constructors
        /// <summary>
        /// Initializes an member of EntityPropertyAttribute.
        /// </summary>
        /// <param name="propertyType">Entity type of the property.</param>
        public EntityPropertyAttribute(EntityPropertyType propertyType)
        {
            _propertyType = propertyType;
        }

        // public properties
        /// <summary>
        /// Gets the type of the entity property.
        /// </summary>
        public EntityPropertyType PropertyType
        {
            get { return _propertyType; }
        }
    }
}