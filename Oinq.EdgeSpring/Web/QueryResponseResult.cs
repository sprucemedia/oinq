using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents the data results from a EdgeSpring REST API response.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryResponseResult
    {
        // private fields
        private IList<Object> _records;

        // constructors
        /// <summary>
        /// Initializes a new QueryResponseResult from a list of records.
        /// </summary>
        /// <param name="records"></param>
        public QueryResponseResult(IList<Object> records)
        {
            _records = records;
        }

        // public properties
        /// <summary>
        /// Gets the records returned in the query result.
        /// </summary>
        [JsonProperty("records")]
        public IEnumerable<Object> Records
        {
            get { return _records; }
        }
    }
}
