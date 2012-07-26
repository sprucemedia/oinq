using System.Linq.Expressions;
using Oinq.Expressions;

namespace Oinq.Translation
{
    /// <summary>
    /// Represents information about GroupBy expressions.
    /// </summary>
    internal class GroupByInfo
    {
        // constructors
        internal GroupByInfo(SourceAlias alias, Expression element)
        {
            Alias = alias;
            Element = element;
        }

        // internal properties
        internal SourceAlias Alias { get; private set; }
        internal Expression Element { get; private set; }
    }
}