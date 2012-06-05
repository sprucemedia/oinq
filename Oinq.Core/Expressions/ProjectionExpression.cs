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
        // private fields
        private LambdaExpression _aggregator;
        private Expression _projector;
        private SelectExpression _source;

        // constructors
        internal ProjectionExpression(SelectExpression source, Expression projector)
            : this(source, projector, null)
        {
        }

        internal ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator)
            : base(PigExpressionType.Projection, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            _source = source;
            _projector = projector;
            _aggregator = aggregator;
        }

        // internal fields
        internal LambdaExpression Aggregator
        {
            get { return _aggregator; }
        }

        internal Expression Projector
        {
            get { return _projector; }
        }

        internal SelectExpression Source
        {
            get { return _source; }
        }
    }
}