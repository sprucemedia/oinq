using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a column declaration in a column projection.
    /// </summary>
    internal class ColumnDeclaration
    {
        // constructors
        internal ColumnDeclaration(String name, Expression expression)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (expression == null)
                throw new ArgumentNullException("expression");
            Expression = expression;
            Name = name;
        }

        // internal properties
        internal Expression Expression { get; private set; }
        internal String Name { get; private set; }
    }
}