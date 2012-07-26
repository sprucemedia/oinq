using System;

namespace Oinq.Expressions
{
    /// <summary>
    /// A custom expression node that represents a reference to a column in a query.
    /// </summary>
    internal class ColumnExpression : AliasedExpression
    {
        // constructors
        internal ColumnExpression(Type type, SourceAlias alias, String name)
            : base(PigExpressionType.Column, type, alias)
        {
            Name = name;
        }

        // internal properties
        internal string Name { get; private set; }
    }
}