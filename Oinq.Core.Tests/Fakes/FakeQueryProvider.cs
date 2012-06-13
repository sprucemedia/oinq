using System;
using System.Collections.Generic;

namespace Oinq.Core.Tests
{
    public class FakeQueryProvider : QueryProvider
    {
        private IList<FakeData> _results;
        private Query<FakeData> _fakeData;

        public FakeQueryProvider(ISource source, IList<FakeData> results)
            : base(source)
        {
            _results = results;
            _fakeData = new Query<FakeData>(this);
        }

        public override Object Execute(String commandText, String[] paramNames, Object[] paramValues, 
            Func<EnumerableDataReader,Object> fnRead)
        {
            return fnRead(new EnumerableDataReader(_results));
        }

        public Query<FakeData> FakeData
        {
            get { return _fakeData; }
        }
    }
}
