using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Core
{
    public enum PigExpressionType
    {
        Source = 1000,
        Column,
        Select,
        Projection,
        IsNull,
        Scalar,
        Aggregate,
        AggregateSubquery,
        Subquery
    }
}
