using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oinq.EdgeSpring.Entity;

namespace Oinq.EdgeSpring.Tests
{
    public class FakeData : IEntity
    {
        public Int32 miles { get; set; }
        public String carrier { get; set; }
    }
}
