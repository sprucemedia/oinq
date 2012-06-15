using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression node representing a join clause
    /// </summary>
    public class JoinExpression : PigExpression
    {
        // constructors
        public JoinExpression(JoinType joinType, Expression left, Expression right, Expression condition)
            : base(PigExpressionType.Join, typeof(void))
        {
            Join = joinType;
            Left = left;
            Right = right;
            Condition = condition;
        }
        
        // public properties
        public JoinType Join { get; private set; }
        public Expression Left { get; private set; }      
        public Expression Right { get; private set; }       
        public new Expression Condition { get; private set; }
    }
}
