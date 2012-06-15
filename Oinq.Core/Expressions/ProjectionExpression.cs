using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression representing the construction of one or more result objects from a 
    /// select expression.
    /// </summary>
    public class ProjectionExpression : PigExpression
    {
        // private fields
        private LambdaExpression _aggregator;
        private Expression _projector;
        private SelectExpression _source;

        // constructors
        public ProjectionExpression(SelectExpression source, Expression projector)
            : this(source, projector, null)
        {
        }

        public ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator)
            : base(PigExpressionType.Projection, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            _source = source;
            _projector = projector;
            _aggregator = aggregator;
        }

        // public fields
        public LambdaExpression Aggregator
        {
            get { return _aggregator; }
        }

        public Expression Projector
        {
            get { return _projector; }
        }

        public SelectExpression Source
        {
            get { return _source; }
        }
    }
}