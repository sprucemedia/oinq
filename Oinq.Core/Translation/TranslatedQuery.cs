using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a LINQ query that has been translated to an EdgeSpring query.
    /// </summary>
    public class TranslatedQuery
    {
        // constructors
        /// <summary>
        /// Initializes a new _instance of the TranslatedQuery class.
        /// </summary>
        /// <param name="commandText">The command text of the query.</param>
        /// <param name="projector">The projector.</param>
        public TranslatedQuery(String commandText, LambdaExpression projector, LambdaExpression aggregator)
        {
            CommandText = commandText;
            Projector = projector;
            Aggregator = aggregator;
        }

        // public properties
        /// <summary>
        /// Gets the command text of the query.
        /// </summary>
        public String CommandText { get; private set; }

        /// <summary>
        /// Gets the projector.
        /// </summary>
        public LambdaExpression Projector { get; private set; }

        /// <summary>
        /// Gets the aggregator.
        /// </summary>
        public LambdaExpression Aggregator { get; private set; }
    }
}
