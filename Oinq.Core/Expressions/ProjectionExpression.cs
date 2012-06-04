using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression representing the construction of one or more result objects from a 
    /// select expression.
    /// </summary>
    internal class ProjectionExpression : PigExpression
    {
        // constructors
        internal ProjectionExpression(SelectExpression source, Expression projector)
            : this(source, projector, null)
        {
        }

        internal ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator)
            : base(PigExpressionType.Projection, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            Source = source;
            Projector = projector;
            Aggregator = aggregator;
        }

        // internal columns
        internal LambdaExpression Aggregator { get; private set; }
        internal String QueryText { get { return QueryFormatter.Format(Source); } }
        internal Expression Projector { get; private set; }
        internal SelectExpression Source { get; private set; }
    }
}