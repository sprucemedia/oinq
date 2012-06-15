using System;

namespace Oinq.Core
{
    /// <summary>
    /// A custom expression node that represents a reference to a column in a query.
    /// </summary>
    public class ColumnExpression : PigExpression
    {
        // constructors
        public ColumnExpression(Type type, SourceAlias alias, String name)
            : base(PigExpressionType.Column, type)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            Alias = alias;
            Name = name;
        }

        // public properties
        public SourceAlias Alias { get; private set; }
        public String Name { get; private set; }
    }
}
