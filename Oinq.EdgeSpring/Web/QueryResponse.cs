using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents a response from the EdgeSpring REST API.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueryResponse
    {
        //private fields
        private QueryResponseResult _result;

        // constructors
        /// <summary>
        /// Initializes a new QueryResponse from a response string.
        /// </summary>
        /// <param name="responseString">The response string.</param>
        public QueryResponse(String responseString)
        {
            _result = BuildQueryResponseResult(responseString);
        }

        // public properties
        /// <summary>
        /// Returns the result object of the response.
        /// </summary>
        [JsonProperty("results")]
        public QueryResponseResult Result
        {
            get { return _result; }
        }

        // private static methods
        private static QueryResponseResult BuildQueryResponseResult(String responseString)
        {
            try
            {
                JObject fullResults = JObject.Parse(responseString);
                var records = fullResults["results"]["records"].ToString();
                return new QueryResponseResult(JsonConvert.DeserializeObject<List<Object>>(records));
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }
    }
}
