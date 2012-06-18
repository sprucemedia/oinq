using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a column declaration in a column projection.
    /// </summary>
    public class ColumnDeclaration
    {
        // private fields
        private Expression _expression;
        private String _name;

        // constructors
        public ColumnDeclaration(String name, Expression expression)
        {
            _expression = expression;
            _name = name;
        }

        // public properties
        public Expression Expression
        {
            get { return _expression; }
        }

        public String Name
        {
            get { return _name; }
        }
    }
}