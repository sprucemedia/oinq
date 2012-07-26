using System;

namespace Oinq.EdgeSpring.Web
{
    /// <summary>
    /// Represents a query to be sent to the EdgeSpring API.
    /// </summary>
    public class Query
    {
        // private fields
        private const String _action = "query";
        private readonly Scope _otherScope;
        private readonly String _queryText;

        // constructors
        /// <summary>
        /// Initializes a new query object from the command text.
        /// </summary>
        /// <param name="queryText">query command text.</param>
        public Query(String queryText)
        {
            _queryText = queryText;
            _otherScope = new Scope();
        }

        // public properties
        /// <summary>
        /// Gets the action name of the query.
        /// </summary>
        public String action
        {
            get { return _action; }
        }

        /// <summary>
        /// Gets the other scope node of the query.
        /// </summary>
        public Scope otherscope
        {
            get { return _otherScope; }
        }

        /// <summary>
        /// Gets the query text.
        /// </summary>
        public String query
        {
            get { return _queryText.Replace('\'', '"'); }
        }
    }

    /// <summary>
    /// Placeholder of the otherscope property.
    /// </summary>
    public class Scope
    {
    }
}