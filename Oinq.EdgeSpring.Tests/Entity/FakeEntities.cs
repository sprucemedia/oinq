using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oinq.EdgeSpring.Entity;

namespace Oinq.EdgeSpring.Tests
{
    public class FakeFullEntity : IEntity
    {
        // Keys
        [EntityProperty(EntityPropertyType.Key)]
        public String Key1 { get; set; }
        [EntityProperty(EntityPropertyType.Key)]
        public String Key2 { get; set; }

        // Dimensions
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim1 { get; set; }
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim2 { get; set; }
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim3 { get; set; }

        // Measures
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea1 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea2 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea3 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea4 { get; set; }
    }

    public class FakeMeasureEntity : IEntity
    {
        // Keys
        [EntityProperty(EntityPropertyType.Key)]
        public String Key1 { get; set; }
        [EntityProperty(EntityPropertyType.Key)]
        public String Key2 { get; set; }

        // Measures
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea1 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea2 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea3 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea4 { get; set; }
    }

    public class FakeDimensionEntity : IEntity
    {
        // Keys
        [EntityProperty(EntityPropertyType.Key)]
        public String Key1 { get; set; }
        [EntityProperty(EntityPropertyType.Key)]
        public String Key2 { get; set; }

        // Dimensions
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim1 { get; set; }
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim2 { get; set; }
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim3 { get; set; }
    }

    public class FakeNoKeyEntity : IEntity
    {
        // Dimensions
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim1 { get; set; }
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim2 { get; set; }
        [EntityProperty(EntityPropertyType.Dimension)]
        public String Dim3 { get; set; }

        // Measures
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea1 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea2 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea3 { get; set; }
        [EntityProperty(EntityPropertyType.Measure)]
        public Int32 Mea4 { get; set; }
    }

    public class FakeKeyEntity : IEntity
    {
        // Keys
        [EntityProperty(EntityPropertyType.Key)]
        public String Key1 { get; set; }
        [EntityProperty(EntityPropertyType.Key)]
        public String Key2 { get; set; }
    }
}
