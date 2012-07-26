using System;
using System.Collections.Generic;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents a response from the EdgeSpring REST API update action.
    /// </summary>
    public class UpdateResponse
    {
        // constructors
        /// <summary>
        /// Initializes a new UpdateResponse.
        /// </summary>
        public UpdateResponse()
        {
        }

        // public properties
        /// <summary>
        /// Gets the type of action in the API request.
        /// </summary>
        public String action { get; set; }

        /// <summary>
        /// Gets the EdgeSpring Edgemart name.
        /// </summary>
        public String edgemart { get; set; }

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
        /// Gets the result string.
        /// </summary>
        public List<String> results { get; set; }

        /// <summary>
        /// Gets the EdgeSpring worker id.
        /// </summary>
        public String workerId { get; set; }
    }
}
