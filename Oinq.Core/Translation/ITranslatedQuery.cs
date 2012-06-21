using System;

namespace Oinq
{
    /// <summary>
    /// Represents an actionable query expression.
    /// </summary>
    public interface ITranslatedQuery
    {
        /// <summary>
        /// Query command text.
        /// </summary>
        String CommandText { get; }

        /// <summary>
        /// Data source for the query.
        /// </summary>
        IDataFile Source { get; }

        /// <summary>
        /// Type of the data.
        /// </summary>
        Type SourceType { get; }
    }
}
