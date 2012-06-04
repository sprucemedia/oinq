using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression node representing a join clause
    /// </summary>
    internal class JoinExpression : PigExpression
    {
        // constructors
        internal JoinExpression(JoinType joinType, Expression left, Expression right, Expression condition)
            : base(PigExpressionType.Join, typeof(void))
        {
            Join = joinType;
            Left = left;
            Right = right;
            Condition = condition;
        }
        
        // internal properties
        internal JoinType Join { get; private set; }
        internal Expression Left { get; private set; }      
        internal Expression Right { get; private set; }       
        internal new Expression Condition { get; private set; }
    }
}
