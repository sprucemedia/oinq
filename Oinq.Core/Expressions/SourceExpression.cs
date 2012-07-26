using System;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    internal class SourceExpression : AliasedExpression
    {
        // constructors
        internal SourceExpression(Type sourceType, SourceAlias alias, String name)
            : base(PigExpressionType.Source, sourceType, alias)
        {
            Name = name;
        }

        // internal properties
        internal string Name { get; private set; }
    }
}