namespace Tools.Models
{
    public class SortModel
    {
        private string UpArrow = "bi bi-arrow-up";
        private string DownArrow = "bi bi-arrow-down";

        public string SortProperty { get; set; }
        public OrderBy OrderBy { get; set; }
        public string SortExpression { get; private set; }

        private List<SortableColumn> sortableColumns = new List<SortableColumn>();

        public void AddColumn(string colName, bool IsDefaultColumn = false)
        {
            SortableColumn tmp = this.sortableColumns.Where(c => c.ColumnName.ToLower() == colName.ToLower())
                .SingleOrDefault();
            if (tmp == null)
                sortableColumns.Add(new SortableColumn() { ColumnName = colName });

            if (IsDefaultColumn == true || sortableColumns.Count == 1)
            {
                SortProperty = colName;
                OrderBy = OrderBy.Ascending;
            }
        }

        public SortableColumn GetColumn(string colName)
        {
            SortableColumn tmp = this.sortableColumns.Where(c => c.ColumnName.ToLower() == colName.ToLower())
                .SingleOrDefault();
            if (tmp == null)
                sortableColumns.Add(new SortableColumn() { ColumnName = colName });
            return tmp;
        }

        public void ApplySort(string sortExpression = "")
        {
            if (sortExpression == null)
                sortExpression = "";

            if (sortExpression == "")
                sortExpression = this.SortProperty;

            sortExpression = sortExpression.ToLower();
            SortExpression = sortExpression;

            foreach (SortableColumn sortableColumn in this.sortableColumns)
            {
                sortableColumn.SortIcon = "";
                sortableColumn.SortExpression = sortableColumn.ColumnName;

                if (sortExpression == sortableColumn.ColumnName.ToLower())
                {
                    this.OrderBy = OrderBy.Ascending;
                    this.SortProperty = sortableColumn.ColumnName;

                    sortableColumn.SortIcon = DownArrow;
                    sortableColumn.SortExpression = $"{sortableColumn.ColumnName}_desc";
                }

                if (sortExpression == $"{sortableColumn.ColumnName.ToLower()}_desc")
                {
                    this.OrderBy = OrderBy.Descending;
                    this.SortProperty = sortableColumn.ColumnName;

                    sortableColumn.SortIcon = UpArrow;
                    sortableColumn.SortExpression = sortableColumn.ColumnName;
                }
            }
        }
    }

    public class SortableColumn
    {
        public string ColumnName { get; set; }
        public string SortExpression { get; set; }
        public string SortIcon { get; set; }
    }

    public enum OrderBy
    { Ascending = 0, Descending = 1 }
}