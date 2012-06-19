using System;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression node that represents a reference to a column in a query.
    /// </summary>
    internal class ColumnExpression : AliasedExpression
    {
        // private fields
        private String _name;

        // constructors
        internal ColumnExpression(Type type, SourceAlias alias, String name)
            : base(PigExpressionType.Column, type, alias)
        {
            _name = name;
        }

        // internal properties
        internal String Name
        {
            get { return _name; }
        }
    }
}