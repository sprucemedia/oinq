﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Oinq.Expressions;

namespace Oinq.Translation
{
    internal class SubqueryRemover : PigExpressionVisitor
    {
        // private fields
        private readonly Dictionary<SourceAlias, Dictionary<String, Expression>> _map;
        private readonly HashSet<SelectExpression> _selectsToRemove;

        // constructors
        private SubqueryRemover(IEnumerable<SelectExpression> selectsToRemove)
        {
            _selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            _map = _selectsToRemove.ToDictionary(d => d.Alias,
                                                 d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
        }

        // internal static methods
        internal static SelectExpression Remove(SelectExpression outerSelect,
                                                IEnumerable<SelectExpression> selectsToRemove)
        {
            return (SelectExpression) new SubqueryRemover(selectsToRemove).Visit(outerSelect);
        }

        internal static ProjectionExpression Remove(ProjectionExpression projection,
                                                    IEnumerable<SelectExpression> selectsToRemove)
        {
            return (ProjectionExpression) new SubqueryRemover(selectsToRemove).Visit(projection);
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
            if (_selectsToRemove.Contains(node))
            {
                return Visit(node.From);
            }
            return base.VisitSelect(node);
        }
    }
}