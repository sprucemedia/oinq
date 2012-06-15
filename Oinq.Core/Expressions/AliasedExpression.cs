using System;

namespace Oinq.Core
{
    public abstract class AliasedExpression : PigExpression
    {
        // constructors
        protected AliasedExpression(PigExpressionType nodeType, Type type, SourceAlias alias)
            : base(nodeType, type)
        {
            Alias = alias;
        }

        // public properties
        public SourceAlias Alias { get; private set; }
    }

    public class SourceAlias
    {
        // constructors
        public SourceAlias()
        {
        }

        public override string ToString()
        {
            return "A:" + GetHashCode();
        }
    }
}
