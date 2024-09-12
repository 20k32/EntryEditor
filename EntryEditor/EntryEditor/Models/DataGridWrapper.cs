using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryEditor.Models
{
    internal sealed class DataGridWrapper
    {
        private readonly DataGrid dataGrid;

        public DataGridWrapper(DataGrid dataGrid) => this.dataGrid = dataGrid;

        public void BeginEdit() => dataGrid.BeginEdit();
        public void CancelEdit() => dataGrid.CancelEdit();
        public void EndEdit() => dataGrid.CommitEdit();
    }
}
