using System;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression node that represents a reference to a column in a query.
    /// </summary>
    internal class ColumnExpression : PigExpression
    {
        // constructors
        internal ColumnExpression(Type type, SourceAlias alias, String name)
            : base(PigExpressionType.Column, type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            Alias = alias;
            Name = name;
        }

        // internal properties
        internal SourceAlias Alias { get; private set; }
        internal String Name { get; private set; }
    }
}
