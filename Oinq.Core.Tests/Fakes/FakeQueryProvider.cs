using System;
using System.Collections.Generic;

namespace Oinq.Tests
{
    public class FakeQueryProvider : QueryProvider
    {
        private IList<AttributedFakeData> _results;

        public FakeQueryProvider(IDataFile source, IList<AttributedFakeData> results)
            : base(source)
        {
            _results = results;
        }

        protected override IList<TResult> Execute<TResult>(ITranslatedQuery translatedQuery)
        {
            return (IList<TResult>)_results;
        }
    }
}
