using System.Collections.Generic;
using System.Linq.Expressions;

namespace Oinq.Expressions
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

        // internal fields
        internal LambdaExpression Aggregator { get; private set; }
        internal Expression Projector { get; private set; }
        internal SelectExpression Source { get; private set; }
    }
}