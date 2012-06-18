using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents information about GroupBy expressions.
    /// </summary>
    public class GroupByInfo
    {
        // constructors
        public GroupByInfo(SourceAlias alias, Expression element)
        {
            Alias = alias;
            Element = element;
        }

        // public properties
        public SourceAlias Alias { get; private set; }
        public Expression Element { get; private set; }
    }
}