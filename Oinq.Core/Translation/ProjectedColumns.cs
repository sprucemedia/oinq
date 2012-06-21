using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq
{
    internal sealed class ProjectedColumns
    {
        // private fields
        private Expression _projector;
        private ReadOnlyCollection<ColumnDeclaration> _columns;

        // constructors
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            _projector = projector;
            _columns = columns;
        }

        // internal properties
        internal Expression Projector
        {
            get { return _projector; }
        }

        internal ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get { return _columns; }
        }
    }
}
