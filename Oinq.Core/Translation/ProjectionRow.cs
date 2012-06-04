using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A ProjectionRow is an abstract over a row-based data source.
    /// </summary>
    public abstract class ProjectionRow
    {
        public abstract Object GetValue(Int32 index);
        public abstract IEnumerable<E> ExecuteSubQuery<E>(LambdaExpression query);
    }
}
