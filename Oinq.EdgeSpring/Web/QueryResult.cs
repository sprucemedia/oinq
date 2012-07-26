using System.Collections.Generic;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents the data results from a EdgeSpring REST API response.
    /// </summary>
    public class QueryResult<T>
    {
        // public properties
        /// <summary>
        /// Gets the result description.
        /// </summary>
        public QueryResultDescription description { get; set; }
        
        /// <summary>
        /// Gets the records returned in the query result.
        /// </summary>
        public List<T> records { get; set; }
    }
}