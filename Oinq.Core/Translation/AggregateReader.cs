using System;
using System.Collections;
using System.Collections.Generic;

namespace Oinq.Core
{
    public abstract class AggregateReader
    {
        public abstract object Read(IEnumerable sequence);
    }

    public class AggregateReader<T, S> : AggregateReader
    {
        Func<IEnumerable<T>, S> _aggregator;

        public AggregateReader(Func<IEnumerable<T>, S> aggregator)
        {
            _aggregator = aggregator;
        }

        public override object Read(IEnumerable sequence)
        {
            return _aggregator((IEnumerable<T>)sequence);
        }
    }
}
