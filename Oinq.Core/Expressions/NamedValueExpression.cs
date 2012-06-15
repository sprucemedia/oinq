using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    public class NamedValueExpression : PigExpression
    {
        // constructors
        public NamedValueExpression(String name, Expression value)
            : base(PigExpressionType.NamedValue, value.Type)
        {
            Name = name;
            Value = value;
        }

        // public properties
        public String Name { get; private set; }
        public Expression Value { get; private set; }
    }
}
