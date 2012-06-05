using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Oinq.EdgeSpring.Web
{
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryResponseResult
    {
        private IList<Object> _records;

        public QueryResponseResult(IList<Object> records)
        {
            _records = records;
        }

        [JsonProperty("records")]
        public IList<Object> Records
        {
            get { return _records; }
        }
    }
}
