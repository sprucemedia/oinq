using System.Collections.ObjectModel;
using System.Reflection;

namespace Oinq.EdgeSpring.Entity
{
    /// <summary>
    /// Interface for entity info.
    /// </summary>
    public interface IEntityInfo
    {
        /// <summary>
        /// Gets the dimensions for the entity.
        /// </summary>
        ReadOnlyCollection<PropertyInfo> Dimensions { get; }
        /// <summary>
        /// Gets the keys for the entity.
        /// </summary>
        ReadOnlyCollection<PropertyInfo> Keys { get; }
        /// <summary>
        /// Gets the measures for the entity.
        /// </summary>
        ReadOnlyCollection<PropertyInfo> Measures { get; }
    }
}