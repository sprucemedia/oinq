using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Oinq.EdgeSpring.Web
{
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryResponse
    {
        private QueryResponseResult _result;

        public QueryResponse(QueryResponseResult result)
        {
            _result = result;
        }

        [JsonProperty("results")]
        public QueryResponseResult Result
        {
            get { return _result; }
        }
    }
}
