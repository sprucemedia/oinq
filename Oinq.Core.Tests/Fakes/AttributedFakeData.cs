using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Tests
{
    [PigSourceMapping("FakeData")]
    public class AttributedFakeData
    {
        [PigMapping("dimension")]
        public String Dim1 { get; set; }
        [PigMapping("measure")]
        public Int32 Mea1 { get; set; }
        [PigIgnore()]
        public Int32 Calc
        {
            get { return Mea1 * 2; }
            private set { }
        }
    }
}
