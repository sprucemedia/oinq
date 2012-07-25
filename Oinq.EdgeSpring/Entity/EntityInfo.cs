using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Oinq.EdgeSpring.Entity
{
    public class EntityInfo : IEntityInfo
    {
        // private fields
        private ReadOnlyCollection<PropertyInfo> _keys;
        private ReadOnlyCollection<PropertyInfo> _dimensions;
        private ReadOnlyCollection<PropertyInfo> _measures;

        // constructors
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
        public ReadOnlyCollection<PropertyInfo> Dimensions
        {
            get { return _dimensions; }
        }

        public ReadOnlyCollection<PropertyInfo> Keys
        {
            get { return _keys; }
        }

        public ReadOnlyCollection<PropertyInfo> Measures
        {
            get { return _measures; }
        }
    }
}
