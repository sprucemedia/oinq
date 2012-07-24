using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.EdgeSpring.Tests
{
    public class FakeData : IUpdateable
    {
        public Int32 miles { get; set; }
        public String carrier { get; set; }

        public IDictionary<string, string> GetKeys()
        {
            throw new NotImplementedException();
        }
    }
}
