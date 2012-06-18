using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public class SubqueryRemover : PigExpressionVisitor
    {
        // private fields
        private HashSet<SelectExpression> _selectsToRemove;
        private Dictionary<SourceAlias, Dictionary<String, Expression>> _map;

        // constructors
        private SubqueryRemover(IEnumerable<SelectExpression> selectsToRemove)
        {
            _selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            _map = _selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
        }

        // public static methods
        public static SelectExpression Remove(SelectExpression outerSelect, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public static SelectExpression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove)
        {
            return (SelectExpression)new SubqueryRemover(selectsToRemove).Visit(outerSelect);
        }

        public static ProjectionExpression Remove(ProjectionExpression projection, params SelectExpression[] selectsToRemove)
        {
            return Remove(projection, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public static ProjectionExpression Remove(ProjectionExpression projection, IEnumerable<SelectExpression> selectsToRemove)
        {
            return (ProjectionExpression)new SubqueryRemover(selectsToRemove).Visit(projection);
        }

        // protected override methods
        protected override Expression VisitColumn(ColumnExpression node)
        {
            Dictionary<String, Expression> nameMap;
            if (_map.TryGetValue(node.Alias, out nameMap))
            {
                Expression expr;
                if (nameMap.TryGetValue(node.Name, out expr))
                {
                    return Visit(expr);
                }
                throw new Exception("Reference to undefined column");
            }
            return node;
        }

        protected override Expression VisitSelect(SelectExpression node)
        {
            if (this._selectsToRemove.Contains(node))
            {
                return Visit(node.From);
            }
            else
            {
                return base.VisitSelect(node);
            }
        }
    }
}