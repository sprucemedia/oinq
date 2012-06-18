using System;
using System.Collections.Generic;

namespace Oinq.Core.Tests
{
    public class FakeQueryProvider : QueryProvider
    {
        private IList<FakeData> _results;
        private Query<FakeData> _fakeData;

        public FakeQueryProvider(IDataFile source, IList<FakeData> results)
            : base(source)
        {
            _results = results;
            _fakeData = new Query<FakeData>(this);
        }

        protected override Object Execute(TranslatedQuery translatedQuery)
        {
            return _results;
        }

        public Query<FakeData> FakeData
        {
            get { return _fakeData; }
        }
    }
}
