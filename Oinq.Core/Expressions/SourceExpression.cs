using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an expression describing the load source.
    /// </summary>
    public class SourceExpression : AliasedExpression
    {
        // private fields
        private String _name;

        // constructors
        public SourceExpression(SourceAlias alias, String name)
            : base(PigExpressionType.Source, typeof(void), alias)
        {
            _name = name;
        }

        // public properties
        public String Name
        {
            get { return _name; }
        }
    }
}