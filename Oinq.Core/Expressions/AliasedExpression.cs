using System;

namespace Oinq.Core
{
    public abstract class AliasedExpression : PigExpression
    {
        private SourceAlias _alias;

        // constructors
        protected AliasedExpression(PigExpressionType nodeType, Type type, SourceAlias alias)
            : base(nodeType, type)
        {
            _alias = alias;
        }

        // public properties
        public SourceAlias Alias
        {
            get { return _alias; }
        }
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