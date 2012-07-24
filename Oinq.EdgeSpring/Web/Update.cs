using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents an update request to be sent to the EdgeSpring API.
    /// </summary>
    public class Update
    {
        // private fields
        private readonly String _action = "update";
        private String _edgeMartUrl;
        private IUpdateable _originalObject;
        private IUpdateable _modifiedObject;

        // constructors
        public Update(String edgemartUrl, IUpdateable originalObject, IUpdateable modifiedObject)
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

            _edgeMartUrl = edgemartUrl;
            _originalObject = originalObject;
            _modifiedObject = modifiedObject;
        }

        // public methods
        public Uri ToUri()
        {
            StringBuilder sb = new StringBuilder(_edgeMartUrl);
            sb.Append(String.Format("&action={0}", _action));

            // filters
            IDictionary<String, String> filters = _originalObject.GetKeys();
            foreach (String key in filters.Keys)
            {
                sb.Append(String.Format("&filters={0}:{1}", key, filters[key]));
            }


            // dims = need old values where different
            // measures = need old values
            // values = need new values where different

            return new Uri(sb.ToString());
        }

        // private methods

    }
}
