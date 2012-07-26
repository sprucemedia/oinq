using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Oinq.Expressions;

namespace Oinq.Translation
{
    internal sealed class ProjectedColumns
    {
        // constructors
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }

        // internal properties
        internal Expression Projector { get; private set; }
        internal ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
    }
}