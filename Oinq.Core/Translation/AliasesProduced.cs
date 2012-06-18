using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class AliasesProduced : PigExpressionVisitor
    {
        HashSet<SourceAlias> aliases;

        private AliasesProduced()
        {
            this.aliases = new HashSet<SourceAlias>();
        }

        public static HashSet<SourceAlias> Gather(Expression source)
        {
            AliasesProduced produced = new AliasesProduced();
            produced.Visit(source);
            return produced.aliases;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            this.aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitSource(Expression node)
        {
            var file = node as SourceExpression;
            this.aliases.Add(file.Alias);
            return node;
        }
    }
}
