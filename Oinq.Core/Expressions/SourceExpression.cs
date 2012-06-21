using System;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    internal class SourceExpression : AliasedExpression
    {
        // private fields
        private String _name;

        // constructors
        internal SourceExpression(SourceAlias alias, String name)
            : base(PigExpressionType.Source, typeof(void), alias)
        {
            _name = name;
        }

        // internal properties
        internal String Name
        {
            get { return _name; }
        }
    }
}