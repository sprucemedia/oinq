using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Oinq.Core
{
    public class SelectQuery : TranslatedQuery
    {
        private LambdaExpression _aggregator;
        private Expression _where;
        private ReadOnlyCollection<OrderByExpression> _orderBy;
        private LambdaExpression _projection;
        private Expression _take;
        private List<ColumnDeclaration> _columns;
        private ReadOnlyCollection<Expression> _groupBy;
        private Stack _commandStack;

        // constructors
        /// <summary>
        /// Initializes a new instance of the SelectQuery class.
        /// </summary>
        /// <param name="collection">The data source being queried.</param>
        /// <param name="sourceType">The source type.</param>
        public SelectQuery(IDataFile source, Type sourceType)
            : base(source, sourceType)
        {
            _commandStack = new Stack();
            _columns = new List<ColumnDeclaration>();
        }

        // public properties
        public ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get { return _columns.AsReadOnly(); }
        }

        public String CommandText
        {
            get { return PigFormatter.Format(this); }
        }

        public Stack CommandStack
        {
            get { return _commandStack; }
        }

        public ReadOnlyCollection<Expression> GroupBy
        {
            get { return _groupBy; }
        }

        /// <summary>
        /// Gets a list of Expressions that defines the sort order (or null if not specified).
        /// </summary>
        public ReadOnlyCollection<OrderByExpression> OrderBy
        {
            get { return _orderBy; }
        }

        /// <summary>
        /// Gets the Expression that defines the projection (or null if not specified).
        /// </summary>
        public LambdaExpression Projection
        {
            get { return _projection; }
        }

        /// <summary>
        /// Gets the Expression that defines how many documents to take (or null if not specified);
        /// </summary>
        public Expression Take
        {
            get { return _take; }
        }

        /// <summary>
        /// Gets the Expression that defines the where clause (or null if not specified).
        /// </summary>
        public Expression Where
        {
            get { return _where; }
        }

        /// <summary>
        /// Translates a LINQ query expression tree.
        /// </summary>
        /// <param name="expression">The LINQ query expression tree.</param>
        public void Translate(Expression expression)
        {
            // when we reach the original Query<T>, we're done
            var sourceExpression = expression as SourceExpression;
            if (sourceExpression != null)
            {
                return;
            }

            SelectExpression selectExpression = expression as SelectExpression;
            if (selectExpression != null)
            {
                _commandStack.Push(selectExpression);
                if (selectExpression.Where != null)
                {
                    _where = selectExpression.Where;
                }
                if (selectExpression.OrderBy != null)
                {
                    _orderBy = selectExpression.OrderBy;
                }
                if (selectExpression.Take != null)
                {
                    _take = selectExpression.Take;
                }
                if (selectExpression.GroupBy != null)
                {
                    _groupBy = selectExpression.GroupBy;
                }
                Translate(selectExpression.From);
                return;
            }

            // try projection
            ProjectionExpression projectionExpression = expression as ProjectionExpression;
            _projection = new ProjectionBuilder().Build(projectionExpression.Projector);
            _aggregator = projectionExpression.Aggregator;
            if (projectionExpression != null)
            {
                _projection = new ProjectionBuilder().Build(projectionExpression.Projector);
                _aggregator = projectionExpression.Aggregator;
                Translate(projectionExpression.Source);
                NewExpression anonymous = projectionExpression.Projector as NewExpression;
                if (anonymous != null && anonymous.Members != null)
                {
                    for (Int32 i = 0, n = anonymous.Arguments.Count; i < n; i++)
                    {
                        Expression expr = anonymous.Arguments[i];
                        String colName = anonymous.Members[i].Name;
                        switch (expr.NodeType)
                        {
                            case (ExpressionType)PigExpressionType.Column:
                                ColumnExpression col = (ColumnExpression)expr;
                                _columns.Add(new ColumnDeclaration(colName, col));
                                break;
                            case (ExpressionType)PigExpressionType.Aggregate:
                                AggregateExpression agg = (AggregateExpression)expr;
                                _columns.Add(new ColumnDeclaration(colName, agg));
                                break;                      
                        }
                    }
                }
                else
                {
                    _columns = projectionExpression.Source.Columns.ToList(); 
                }        
                return;
            }

            var message = string.Format("Don't know how to translate expression: {0}.", expression.NodeType.ToString());
            throw new NotSupportedException(message);
        }

        private Int32 ToInt32(Expression expression)
        {
            if (expression.Type != typeof(int))
            {
                throw new ArgumentOutOfRangeException("expression", "Expected an Expression of Type Int32.");
            }
            var constantExpression = expression as ConstantExpression;
            if (constantExpression == null)
            {
                throw new ArgumentOutOfRangeException("expression", "Expected a ConstantExpression.");
            }
            return (Int32)constantExpression.Value;
        }

        private static Expression StripQuote(Expression node)
        {
            while (node.NodeType == ExpressionType.Quote)
            {
                node = ((UnaryExpression)node).Operand;
            }
            return node;
        }
    }
}
