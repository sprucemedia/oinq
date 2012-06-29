using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Tests
{
    public class FakeDataSource : IDataFile
    {
        public string Name
        {
            get { return "FakeData"; }
        }

        public string AbsolutePath
        {
            get { throw new NotImplementedException(); }
        }
    }
}
