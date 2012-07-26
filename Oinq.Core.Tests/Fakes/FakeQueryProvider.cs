using System;
using System.Collections.Generic;
using Oinq.Translation;

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

        protected override Object Execute<TResult>(ITranslatedQuery translatedQuery)
        {
            return (IList<TResult>)_results;
        }
    }
}
