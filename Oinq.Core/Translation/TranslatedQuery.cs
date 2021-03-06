﻿using System;
using Oinq.Pig;

namespace Oinq.Translation
{
    /// <summary>
    /// Represents a LINQ query that has been translated to a Pig query.
    /// </summary>
    public abstract class TranslatedQuery : ITranslatedQuery
    {
        // private fields
        private readonly IDataFile _source;
        private readonly Type _sourceType;

        // constructors
        /// <summary>
        /// Initializes a new member of the TranslatedQuery class.
        /// </summary>
        /// <param name="source">The data _source being queried.</param>
        /// <param name="sourceType">The _source type being queried.</param>
        protected TranslatedQuery(IDataFile source, Type sourceType)
        {
            _source = source;
            _sourceType = sourceType;
        }

        // public properties

        #region ITranslatedQuery Members

        /// <summary>
        /// Pig-based command text for the query.
        /// </summary>
        public String CommandText
        {
            get { return PigFormatter.Format(this); }
        }

        /// <summary>
        /// Gets the data _source being queried.
        /// </summary>
        public IDataFile Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Get the _source type being queried.
        /// </summary>
        public Type SourceType
        {
            get { return _sourceType; }
        }

        #endregion
    }
}