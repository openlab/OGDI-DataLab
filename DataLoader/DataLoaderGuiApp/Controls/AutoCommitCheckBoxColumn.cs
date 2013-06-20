using System.Windows;
using System.Windows.Controls;
using DataGridCell = System.Windows.Controls.DataGridCell;
using DataGridCheckBoxColumn = System.Windows.Controls.DataGridCheckBoxColumn;

namespace Ogdi.Data.DataLoaderGuiApp.Controls
{
    public class AutoCommitCheckBoxColumn : DataGridCheckBoxColumn
    {
        #region Implementation

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CommitCellEdit((FrameworkElement)sender);
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            CommitCellEdit((FrameworkElement)sender);
        }

        #endregion

        #region DataGridCheckBoxColumn overrides

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var checkBox = (CheckBox)base.GenerateEditingElement(cell, dataItem);

            checkBox.Checked += checkBox_Checked;
            checkBox.Unchecked += checkBox_Unchecked;

            return checkBox;
        }

        #endregion
    }
}
