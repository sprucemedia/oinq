using System;

namespace Oinq.Core
{
    /// <summary>
    /// Interface to represent a Pig data source.
    /// </summary>
    public interface IDataFile
    {
        /// <summary>
        /// Name of the data file.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Absolute path of the data file.
        /// </summary>
        String AbsolutePath { get; }
    }
}
