using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Oinq.Expressions;

namespace Oinq.Translation
{
    /// <summary>
    /// Represents a LINQ select-style query that has been translated to a Pig query.
    /// </summary>
    public class SelectQuery : TranslatedQuery
    {
        private readonly Stack _commandStack;
        private readonly List<JoinExpression> _joins;
        private readonly List<SourceExpression> _sources;
        private List<ColumnDeclaration> _columns;
        private ReadOnlyCollection<Expression> _groupBy;
        private ReadOnlyCollection<OrderByExpression> _orderBy;
        private Expression _take;
        private Expression _where;

        // constructors
        /// <summary>
        /// Initializes a new member of the SelectQuery class.
        /// </summary>
        /// <param name="source">The data source being queried.</param>
        /// <param name="sourceType">The source type.</param>
        public SelectQuery(IDataFile source, Type sourceType)
            : base(source, sourceType)
        {
            _commandStack = new Stack();
            _columns = new List<ColumnDeclaration>();
            _sources = new List<SourceExpression>();
            _joins = new List<JoinExpression>();
        }

        // internal properties
        internal ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get { return _columns.AsReadOnly(); }
        }

        internal Stack CommandStack
        {
            get { return _commandStack; }
        }

        internal ReadOnlyCollection<Expression> GroupBy
        {
            get { return _groupBy; }
        }

        internal ReadOnlyCollection<JoinExpression> Joins
        {
            get { return _joins.AsReadOnly(); }
        }

        internal ReadOnlyCollection<SourceExpression> Sources
        {
            get { return _sources.AsReadOnly(); }
        }

        /// <summary>
        /// Gets a list of Expressions that defines the sort order (or null if not specified).
        /// </summary>
        internal ReadOnlyCollection<OrderByExpression> OrderBy
        {
            get { return _orderBy; }
        }

        /// <summary>
        /// Gets the Expression that defines how many documents to take (or null if not specified);
        /// </summary>
        internal Expression Take
        {
            get { return _take; }
        }

        /// <summary>
        /// Gets the Expression that defines the where clause (or null if not specified).
        /// </summary>
        internal Expression Where
        {
            get { return _where; }
        }

        /// <summary>
        /// Translates a LINQ query expression tree.
        /// </summary>
        /// <param name="expression">The LINQ query expression tree.</param>
        internal void Translate(Expression expression)
        {
            // when we reach the original Query<T>, we're done
            var sourceExpression = expression as SourceExpression;
            if (sourceExpression != null)
            {
                _sources.Add(sourceExpression);
                return;
            }

            var joinExpression = expression as JoinExpression;
            if (joinExpression != null)
            {
                _joins.Add(joinExpression);
                Translate(joinExpression.Left);
                Translate(joinExpression.Right);
                return;
            }

            var selectExpression = expression as SelectExpression;
            if (selectExpression != null)
            {
                var from = selectExpression.From as AliasedExpression;
                if (from != null)
                {
                    _commandStack.Push(selectExpression);
                }
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
            var projectionExpression = expression as ProjectionExpression;
            if (projectionExpression != null)
            {
                Translate(projectionExpression.Source);
                var anonymous = projectionExpression.Projector as NewExpression;
                if (anonymous != null && anonymous.Members != null)
                {
                    for (Int32 i = 0, n = anonymous.Arguments.Count; i < n; i++)
                    {
                        _columns.Add(new ColumnDeclaration(anonymous.Members[i].Name, anonymous.Arguments[i]));
                    }
                }
                else
                {
                    var member = projectionExpression.Projector as MemberInitExpression;
                    if (member != null)
                    {
                        foreach (MemberBinding binding in member.Bindings)
                        {
                            var assignment = binding as MemberAssignment;
                            if (assignment != null)
                                _columns.Add(new ColumnDeclaration(binding.Member.Name, assignment.Expression));
                        }
                    }
                    else
                    {
                        _columns = projectionExpression.Source.Columns.ToList();
                    }
                }
                return;
            }

            string message = String.Format("Don't know how to translate expression: {0}.",
                                           expression.NodeType.ToString());
            throw new NotSupportedException(message);
        }
    }
}