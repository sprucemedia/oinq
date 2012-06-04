using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    internal class SourceExpression : AliasedExpression
    {
        // constructors
        internal SourceExpression(SourceAlias alias, String name)
            : base(PigExpressionType.Source, typeof(void), alias)
        {
            Name = name;
        }

        // internal properties
        internal String Name { get; private set; }
    }
}
