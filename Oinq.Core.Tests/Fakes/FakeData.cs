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
        public String DimDesc2 { get; set; }
    }

    public class FakeProjection
    {
        public String Key { get; set; }
        public Int32 Measure { get; set; }
        public String Description { get; set; }
    }

    public class FakeProjectionInt
    {
        public Int32 Key { get; set; }
        public Int32 Measure { get; set; }
        public String Description { get; set; }
    }
}