using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Oinq.EdgeSpring.Entity
{
    /// <summary>
    /// Extension methods for IEntity.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Gets information about entity properties.
        /// </summary>
        /// <param name="entity">An instance of IEntity.</param>
        /// <returns>IEntityInfo.</returns>
        public static IEntityInfo GetEntityProperties(this IEntity entity)
        {
            IList<PropertyInfo> keys = new List<PropertyInfo>();
            IList<PropertyInfo> dimensions = new List<PropertyInfo>();
            IList<PropertyInfo> measures = new List<PropertyInfo>();
            Type entityType = entity.GetType();
            
            IList<PropertyInfo> properties = entityType.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(EntityPropertyAttribute), true).Count() == 1)
                .ToList();

            foreach (PropertyInfo property in properties)
            {
                EntityPropertyType propertyType = property.GetCustomAttributes(typeof(EntityPropertyAttribute), true)
                    .Cast<EntityPropertyAttribute>().Single().PropertyType;
                switch (propertyType)
                {
                    case EntityPropertyType.Dimension:
                        dimensions.Add(property);
                        break;
                    case EntityPropertyType.Measure:
                        measures.Add(property);
                        break;
                    case EntityPropertyType.Key:
                        keys.Add(property);
                        break;
                    default:
                        throw new IndexOutOfRangeException("An invalid entity property type of was specified.");
                }
            }
            ValidateEntityInfo(keys, dimensions, measures);
            return new EntityInfo(keys, dimensions, measures);
        }

        // private methods
        private static void ValidateEntityInfo(IList<PropertyInfo> keys, IList<PropertyInfo> dimensions, IList<PropertyInfo> measures)
        {
            // There must be at least one key.
            if (keys.Count == 0)
            {
                throw new ArgumentException("The entity does not contain any defined keys.");
            }
            // Either the dimensions or measures must be populated.
            if (dimensions.Count == 0 && measures.Count == 0)
            {
                throw new ArgumentException("The entity does not contain any defined dimensions or measures.");
            }
        }
    }
}
