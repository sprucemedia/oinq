using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a LINQ query that has been translated to an EdgeSpring query.
    /// </summary>
    internal class TranslatedQuery
    {
        // constructors
        /// <summary>
        /// Initializes a new _instance of the TranslatedQuery class.
        /// </summary>
        /// <param name="commandText">The command text of the query.</param>
        /// <param name="projector">The projector.</param>
        internal TranslatedQuery(String commandText, LambdaExpression projector, LambdaExpression aggregator)
        {
            CommandText = commandText;
            Projector = projector;
            Aggregator = aggregator;
        }

        // internal properties
        /// <summary>
        /// Gets the command text of the query.
        /// </summary>
        internal String CommandText { get; private set; }

        /// <summary>
        /// Gets the projector.
        /// </summary>
        internal LambdaExpression Projector { get; private set; }

        /// <summary>
        /// Gets the aggregator.
        /// </summary>
        internal LambdaExpression Aggregator { get; private set; }
    }
}
