using System;
using Newtonsoft.Json;

namespace Oinq.EdgeSpring.Web
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Query
    {
        // private
        private readonly String _action = "query";
        private readonly Object _otherScope;
        private readonly String _queryText;

        // constructors
        /// <summary>
        /// Initializes a new Query object from the command text.
        /// </summary>
        /// <param name="queryText">Query command text.</param>
        public Query(String queryText)
        {
            _queryText = queryText;
            _otherScope = new Object();
        }

        // public methods
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        // public properties
        /// <summary>
        /// Gets the action name of the query.
        /// </summary>
        [JsonProperty(PropertyName = "action", Order = 1)]
        public String Action
        {
            get { return _action; }
        }

        /// <summary>
        /// Gets the other scope node of the query.
        /// </summary>
        [JsonProperty(PropertyName = "otherscope", Order = 3)]
        public Object OtherScope
        {
            get { return _otherScope; }
        }

        /// <summary>
        /// Gets the query text.
        /// </summary>
        [JsonProperty(PropertyName = "query", Order = 2)]
        public String QueryText
        {
            get { return _queryText; }
        }
    }
}
