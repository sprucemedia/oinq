using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    internal class NamedValueExpression : PigExpression
    {
        // constructors
        internal NamedValueExpression(String name, Expression value)
            : base(PigExpressionType.NamedValue, value.Type)
        {
            Name = name;
            Value = value;
        }

        // internal properties
        internal String Name { get; private set; }
        internal Expression Value { get; private set; }
    }
}
