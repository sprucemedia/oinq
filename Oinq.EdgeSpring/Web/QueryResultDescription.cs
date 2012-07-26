using System;
using System.Collections.Generic;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents the query result description.
    /// </summary>
    public class QueryResultDescription
    {
        /// <summary>
        /// Gets a list of the dimensions of the query.
        /// </summary>
        public List<String> dimensions { get; set; }

        /// <summary>
        /// Gets a list of expression used in the query.
        /// </summary>
        public List<String> expressions { get; set; }

        /// <summary>
        /// Gets a list of the filters used in the query.
        /// </summary>
        public List<String> filters { get; set; }

        /// <summary>
        /// Gets a list of measures used in the query.
        /// </summary>
        public List<String> measures { get; set; }

        /// <summary>
        /// Gets of list of values in the query.
        /// </summary>
        public List<String> values { get; set; }
    }
}
