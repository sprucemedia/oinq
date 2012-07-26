using System;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents a response from the EdgeSpring REST API query action.
    /// </summary>
    public class QueryResponse<T>
    {
        // constructors
        /// <summary>
        /// Initializes a new QueryResponse.
        /// </summary>
        public QueryResponse()
        {
        }

        // public properties
        /// <summary>
        /// Gets the type of action in the API request.
        /// </summary>
        public String action { get; set; }

        /// <summary>
        /// Gets the row limit on the query.
        /// </summary>
        public Int32 limit { get; set; }

        /// <summary>
        /// Gets the scope of the request.
        /// </summary>
        public Scope otherscope { get; set; }

        /// <summary>
        /// Gets the query text of the query request.
        /// </summary>
        public String query { get; set; }

        /// <summary>
        /// Gets the query execution time from the output.
        /// </summary>
        public String query_time { get; set; }

        /// <summary>
        /// Gets the EdgeSpring query id of the request.
        /// </summary>
        public Int32 queryId { get; set; }

        /// <summary>
        /// Gets the EdgeSpring response id.
        /// </summary>
        public String responseId { get; set; }

        /// <summary>
        /// Gets the results of the query.
        /// </summary>
        public QueryResult<T> results { get; set; }
    }
}
