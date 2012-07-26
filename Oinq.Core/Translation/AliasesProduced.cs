using System.Collections.Generic;
using System.Linq.Expressions;
using Oinq.Expressions;

namespace Oinq.Translation
{
    /// <summary>
    /// Returns the set of all _aliases produced by a query source
    /// </summary>
    internal class AliasesProduced : PigExpressionVisitor
    {
        // private fields
        private readonly HashSet<SourceAlias> _aliases;

        // constructors
        private AliasesProduced()
        {
            _aliases = new HashSet<SourceAlias>();
        }

        // internal static methods
        internal static HashSet<SourceAlias> Gather(Expression source)
        {
            var produced = new AliasesProduced();
            produced.Visit(source);
            return produced._aliases;
        }

        // protected methods
        protected override Expression VisitSelect(SelectExpression select)
        {
            _aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitSource(Expression node)
        {
            var file = node as SourceExpression;
            if (file != null) _aliases.Add(file.Alias);
            return node;
        }
    }
}