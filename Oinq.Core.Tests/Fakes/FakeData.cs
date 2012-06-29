using System;

namespace Oinq.Tests
{
    public class FakeData : IAddable
    {
        public String Dim1 { get; set; }
        public Int32 Mea1 { get; set; }
    }

    public class FakeDataMeta
    {
        public String Dim1 { get; set; }
        public String DimDesc { get; set; }
    }
}