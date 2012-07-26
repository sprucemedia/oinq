using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Oinq.EdgeSpring.Entity;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Enumeration of update types.
    /// </summary>
    public enum UpdateType
    {
        Dimension,
        Measure
    }

    /// <summary>
    /// Represents an update request to be sent to the EdgeSpring API.
    /// </summary>
    public class Update
    {
        // private fields
        private const String ACTION = "action";
        private const String ACTION_TYPE = "update";
        private const String FILTER = "filters";
        private const String DIMS = "dims";
        private const String MEASURES = "measures";
        private const String VALUES = "values";

        private String _edgeMartUrl;
        private IEntity _originalObject;
        private IEntity _modifiedObject;
        private IEntityInfo _entityInfo;

        // constructors
        /// <summary>
        /// Initializes an update request.
        /// </summary>
        /// <param name="edgemartUrl">Url of the Edgemart to be updated.</param>
        /// <param name="originalObject">The original object to be updated.</param>
        /// <param name="modifiedObject">The modified object.</param>
        /// <param name="entityInfo">Entify Info.</param>
        public Update(String edgemartUrl, IEntity originalObject, IEntity modifiedObject, IEntityInfo entityInfo)
        {
            if (String.IsNullOrEmpty(edgemartUrl))
            {
                throw new ArgumentNullException("edgemart");
            }
            if (originalObject == null)
            {
                throw new ArgumentNullException("originalObject");
            }
            if (modifiedObject == null)
            {
                throw new ArgumentNullException("modifiedObject");
            }
            if (entityInfo == null)
            {
                throw new ArgumentNullException("entityInfo");
            }

            _edgeMartUrl = edgemartUrl;
            _originalObject = originalObject;
            _modifiedObject = modifiedObject;
            _entityInfo = entityInfo;
        }

        // public methods
        /// <summary>
        /// Generates a URI for the REST-ful update request.
        /// </summary>
        /// <param name="updateType">The type of update.</param>
        /// <returns>The Uri.</returns>
        public Uri ToUri(UpdateType updateType)
        {
            StringBuilder sb = new StringBuilder(_edgeMartUrl);
            sb.Append(String.Format("&{0}={1}", ACTION, ACTION_TYPE));

            // filters
            IList<PropertyInfo> filters = _entityInfo.Keys;
            foreach (PropertyInfo pi in filters)
            {
                sb.Append(GetKeyValuePairString(FILTER, pi.Name, pi.GetValue(_originalObject, null).ToString()));
            }

            IList<PropertyInfo> fields = (updateType == UpdateType.Dimension) ? _entityInfo.Dimensions : _entityInfo.Measures;
            foreach (PropertyInfo pi in fields)
            {
                String fieldType = (updateType == UpdateType.Dimension) ? DIMS : MEASURES;
                String originalValue = pi.GetValue(_originalObject, null).ToString();
                String modifiedValue = pi.GetValue(_modifiedObject, null).ToString();
                if (modifiedValue != originalValue)
                {                
                    sb.Append(GetKeyValuePairString(fieldType, pi.Name, originalValue));
                    sb.Append(GetKeyValuePairString(VALUES, pi.Name, modifiedValue));
                }
            }
            return new Uri(sb.ToString());
        }

        // private methods
        private static String GetKeyValuePairString(String type, String key, String value)
        {
            return String.Format("&{0}={1}:{2}", type, key, value);
        }
    }
}
