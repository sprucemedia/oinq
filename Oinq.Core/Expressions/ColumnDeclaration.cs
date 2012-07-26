using System;
using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents a column declaration in a column projection.
    /// </summary>
    internal class ColumnDeclaration
    {
        // constructors
        internal ColumnDeclaration(String name, Expression expression)
        {
            Expression = expression;
            Name = name;
        }

        // internal properties
        internal Expression Expression { get; private set; }
        internal string Name { get; private set; }
    }
}