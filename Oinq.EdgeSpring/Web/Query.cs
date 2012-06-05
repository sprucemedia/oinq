using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Oinq.EdgeSpring.Web
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Query
    {
        private readonly String _action = "query";
        private readonly Object _otherScope;
        private readonly String _queryText;

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
        [JsonProperty(PropertyName = "action", Order = 1)]
        public String Action
        {
            get { return _action; }
        }

        [JsonProperty(PropertyName = "otherscope", Order = 3)]
        public Object OtherScope
        {
            get { return _otherScope; }
        }

        [JsonProperty(PropertyName = "query", Order = 2)]
        public String QueryText
        {
            get { return _queryText; }
        }
    }
}
