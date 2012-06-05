using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oinq.Core
{
    public interface ISource
    {
        String Name { get; }
        String Path { get; }
    }
}
