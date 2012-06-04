using System;

namespace Oinq.Core
{
    internal abstract class AliasedExpression : PigExpression
    {
        // constructors
        protected AliasedExpression(PigExpressionType nodeType, Type type, SourceAlias alias)
            : base(nodeType, type)
        {
            Alias = alias;
        }

        // public properties
        internal SourceAlias Alias { get; private set; }
    }

    internal class SourceAlias
    {
        // constructors
        internal SourceAlias()
        {
        }

        public override string ToString()
        {
            return "A:" + GetHashCode();
        }
    }
}
