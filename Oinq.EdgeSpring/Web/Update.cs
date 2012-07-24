using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents an update request to be sent to the EdgeSpring API.
    /// </summary>
    public class Update<T> where T : class
    {
        // private fields
        private readonly String _action = "update";
        private EdgeMart<T> _edgeMart;
        private T _modifiedObject;

        // constructors
        public Update(EdgeMart<T> edgemart, T originalObject, T modifiedObject)
        {
            if (edgemart == null)
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

            _edgeMart = edgemart;
            _modifiedObject = modifiedObject;
        }

        // public methods
        public Uri ToUri()
        {
            StringBuilder sb = new StringBuilder(_edgeMart.UrlString);
            sb.Append(String.Format("&action={0}", _action));

            // filter = need keys
            // dims = need old values where different
            // measures = need old values
            // values = need new values where different

            return new Uri(sb.ToString());
        }

        // private methods

    }
}
