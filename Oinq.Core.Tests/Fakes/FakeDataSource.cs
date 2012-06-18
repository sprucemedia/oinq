using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Core.Tests
{
    public class FakeDataSource : IDataFile
    {
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string AbsolutePath
        {
            get { throw new NotImplementedException(); }
        }
    }
}
