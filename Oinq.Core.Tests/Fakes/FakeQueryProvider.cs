using System;
using System.Collections.Generic;

namespace Oinq.Tests
{
    public class FakeQueryProvider : QueryProvider
    {
        private IEnumerable<Object> _results;

        public FakeQueryProvider(IDataFile source, IEnumerable<Object> results)
            : base(source)
        {
            _results = results;
        }

        protected override Object Execute(TranslatedQuery translatedQuery)
        {
            return _results;
        }

    }
}
