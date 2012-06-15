using System;

namespace Oinq.Core
{
    public interface IDataFile
    {
        String Name { get; }
        String AbsolutePath { get; }
    }
}
