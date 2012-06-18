using System;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression node that represents a reference to a column in a query.
    /// </summary>
    public class ColumnExpression : AliasedExpression
    {
        // private fields
        private String _name;

        // constructors
        public ColumnExpression(Type type, SourceAlias alias, String name)
            : base(PigExpressionType.Column, type, alias)
        {
            _name = name;
        }

        // public properties
        public String Name
        {
            get { return _name; }
        }
    }
}