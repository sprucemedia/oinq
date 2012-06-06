using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Oinq.Core;

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
        public IEnumerable<Object> Records
        {
            get { return _records; }
        }
    }
}
