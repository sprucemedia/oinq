using System;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Represents a column declaration in a column projection.
    /// </summary>
    internal class ColumnDeclaration
    {
        // private fields
        private Expression _expression;
        private String _name;

        // constructors
        internal ColumnDeclaration(String name, Expression expression)
        {
            _expression = expression;
            _name = name;
        }

        // internal properties
        internal Expression Expression
        {
            get { return _expression; }
        }

        internal String Name
        {
            get { return _name; }
        }
    }
}