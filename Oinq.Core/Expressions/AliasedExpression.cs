using System;

namespace Oinq.Core
{
    /// <summary>
    /// Abstract type representing a Pig expression with an alias.
    /// </summary>
    internal abstract class AliasedExpression : PigExpression
    {
        private SourceAlias _alias;

        // constructors
        protected AliasedExpression(PigExpressionType nodeType, Type type, SourceAlias alias)
            : base(nodeType, type)
        {
            _alias = alias;
        }

        // internal properties
        internal SourceAlias Alias
        {
            get { return _alias; }
        }
    }

    /// <summary>
    /// Represents an alias on the data source.
    /// </summary>
    internal class SourceAlias
    {
        // constructors
        internal SourceAlias()
        {
        }

        // internal properties
        public override String ToString()
        {
            return "A:" + GetHashCode();
        }
    }
}