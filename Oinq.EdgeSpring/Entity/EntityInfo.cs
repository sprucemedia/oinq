using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Oinq.EdgeSpring.Entity
{
    /// <summary>
    /// Meta-data class describing IEntity.
    /// </summary>
    public class EntityInfo : IEntityInfo
    {
        // private fields
        private ReadOnlyCollection<PropertyInfo> _keys;
        private ReadOnlyCollection<PropertyInfo> _dimensions;
        private ReadOnlyCollection<PropertyInfo> _measures;

        // constructors
        /// <summary>
        /// Initializes a new instance of EntityInfo.
        /// </summary>
        /// <param name="keys">A collection of keys.</param>
        /// <param name="dimensions">A collection of dimensions.</param>
        /// <param name="measures">A collection of measures.</param>
        public EntityInfo(IEnumerable<PropertyInfo> keys, IEnumerable<PropertyInfo> dimensions, IEnumerable<PropertyInfo> measures)
        {
            _keys = keys as ReadOnlyCollection<PropertyInfo>;
            _dimensions = dimensions as ReadOnlyCollection<PropertyInfo>;
            _measures = measures as ReadOnlyCollection<PropertyInfo>;

            if (_keys == null)
            {
                _keys = new List<PropertyInfo>(keys).AsReadOnly();
            }
            if (_dimensions == null)
            {
                _dimensions = new List<PropertyInfo>(dimensions).AsReadOnly();
            }
            if (_measures == null)
            {
                _measures = new List<PropertyInfo>(measures).AsReadOnly();
            }
        }

        // public properties
        /// <summary>
        /// Gets the dimensions of IEntity.
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> Dimensions
        {
            get { return _dimensions; }
        }

        /// <summary>
        /// Gets the keys of IEntity.
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// Gets the measures of IEntity.
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> Measures
        {
            get { return _measures; }
        }
    }
}
