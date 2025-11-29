using System.Windows.Forms;

namespace StudentManagement.Helpers
{
    public static class DataGridViewExtensions
    {
        public static void TrySetColumnWidth(this DataGridView grid, string columnName, int width)
        {
            if (grid == null || grid.IsDisposed) return;
            if (grid.Columns == null || grid.Columns.Count == 0) return;
            if (!grid.Columns.Contains(columnName)) return;
            var col = grid.Columns[columnName];
            if (col != null)
            {
                col.Width = width;
            }
        }

        public static void TrySetColumnVisible(this DataGridView grid, string columnName, bool visible)
        {
            if (grid == null || grid.IsDisposed) return;
            if (grid.Columns == null || grid.Columns.Count == 0) return;
            if (!grid.Columns.Contains(columnName)) return;
            var col = grid.Columns[columnName];
            if (col != null)
            {
                col.Visible = visible;
            }
        }

        public static void TrySetHeaderText(this DataGridView grid, string columnName, string headerText)
        {
            if (grid == null || grid.IsDisposed) return;
            if (grid.Columns == null || grid.Columns.Count == 0) return;
            if (!grid.Columns.Contains(columnName)) return;
            var col = grid.Columns[columnName];
            if (col != null)
            {
                col.HeaderText = headerText;
            }
        }
    }
}
