using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    public class SourceExpression : AliasedExpression
    {
        // constructors
        public SourceExpression(SourceAlias alias, String name)
            : base(PigExpressionType.Source, typeof(void), alias)
        {
            Name = name;
        }

        // public properties
        public String Name { get; private set; }
    }
}
