using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Oinq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Nominator is a class that walks an expression tree bottom-up, determining the set of
    /// candidate expressions that are possible _columns of a select expression.
    /// </summary>
    internal class Nominator : PigExpressionVisitor
    {
        // private fields
        private Func<Expression, Boolean> _canBeColumn;
        private HashSet<Expression> _candidates;
        private Boolean _isBlocked;

        // constructors
        private Nominator(Func<Expression, Boolean> canBeColumn)
        {
            _canBeColumn = canBeColumn;
            _candidates = new HashSet<Expression>();
            _isBlocked = false;
        }

        // internal static methods
        internal static HashSet<Expression> Nominate(Func<Expression, Boolean> canBeColumn, Expression expression)
        {
            Nominator nominator = new Nominator(canBeColumn);
            nominator.Visit(expression);
            return nominator._candidates;
        }

        // protected override methods
        protected override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                Boolean wasBlocked = _isBlocked;
                _isBlocked = false;
                base.Visit(expression);
                if (!_isBlocked)
                {
                    if (_canBeColumn(expression))
                    {
                        _candidates.Add(expression);
                    }
                    else
                    {
                        _isBlocked = true;
                    }
                }
                _isBlocked |= wasBlocked;
            }
            return expression;
        }
    }
}