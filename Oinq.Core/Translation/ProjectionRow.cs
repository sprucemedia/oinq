using System;

namespace Oinq
{
    /// <summary>
    /// Represents a row in the query result set.
    /// </summary>
    public abstract class ProjectionRow
    {
        /// <summary>
        /// Gets a value from the query result set.
        /// </summary>
        /// <param name="index">Index of the value to retrieve.</param>
        /// <returns>A query result value.</returns>
        public abstract Object GetValue(Int32 index);
    }
}
