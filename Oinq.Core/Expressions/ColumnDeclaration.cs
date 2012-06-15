using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a column declaration in a column projection.
    /// </summary>
    public class ColumnDeclaration
    {
        // constructors
        public ColumnDeclaration(String name, Expression expression)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (expression == null)
                throw new ArgumentNullException("expression");
            Expression = expression;
            Name = name;
        }

        // public properties
        public Expression Expression { get; private set; }
        public String Name { get; private set; }
    }
}