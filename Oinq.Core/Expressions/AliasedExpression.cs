using System;

namespace Oinq.Expressions
{
    /// <summary>
    /// Abstract type representing a Pig expression with an alias.
    /// </summary>
    internal abstract class AliasedExpression : PigExpression
    {
        // constructors
        protected AliasedExpression(PigExpressionType nodeType, Type type, SourceAlias alias)
            : base(nodeType, type)
        {
            Alias = alias;
        }

        // internal properties
        internal SourceAlias Alias { get; private set; }
    }

    /// <summary>
    /// Represents an alias on the data source.
    /// </summary>
    internal class SourceAlias
    {
        // internal properties
        public override String ToString()
        {
            return "A:" + GetHashCode();
        }
    }
}