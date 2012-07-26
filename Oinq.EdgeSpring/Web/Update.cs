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
        private const String Action = "action";
        private const String ActionType = "update";
        private const String Filter = "filters";
        private const String Dims = "dims";
        private const String Measures = "measures";
        private const String Values = "values";

        private readonly String _edgeMartUrl;
        private readonly IEntity _originalObject;
        private readonly IEntity _modifiedObject;
        private readonly IEntityInfo _entityInfo;

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
                throw new ArgumentNullException("edgemartUrl");
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
            var sb = new StringBuilder(_edgeMartUrl);
            sb.Append(String.Format("&{0}={1}", Action, ActionType));

            // filters
            IList<PropertyInfo> filters = _entityInfo.Keys;
            foreach (PropertyInfo pi in filters)
            {
                sb.Append(GetKeyValuePairString(Filter, pi.Name, pi.GetValue(_originalObject, null).ToString()));
            }

            IList<PropertyInfo> fields = (updateType == UpdateType.Dimension) ? _entityInfo.Dimensions : _entityInfo.Measures;
            foreach (PropertyInfo pi in fields)
            {
                String fieldType = (updateType == UpdateType.Dimension) ? Dims : Measures;
                String originalValue = pi.GetValue(_originalObject, null).ToString();
                String modifiedValue = pi.GetValue(_modifiedObject, null).ToString();
                if (modifiedValue != originalValue)
                {                
                    sb.Append(GetKeyValuePairString(fieldType, pi.Name, originalValue));
                    sb.Append(GetKeyValuePairString(Values, pi.Name, modifiedValue));
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
