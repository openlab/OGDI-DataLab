using System.Windows.Controls;
using Ogdi.Data.DataLoaderGuiApp.ViewModels;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Reflection;
using System;
using System.Windows;
using Ogdi.Data.DataLoader;
using System.Windows.Input;

namespace Ogdi.Data.DataLoaderGuiApp.Views
{
    public partial class ColumnsMetadataControlView
    {
        public ColumnsMetadataControlView()
        {
            InitializeComponent();
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataGrid2.SelectedCells.Count > 0)
            {
                System.Windows.Controls.DataGridCell cell = GetCell(DataGrid2.SelectedIndex,NamespaceColumn.DisplayIndex);
                if (((ComboBox)sender).SelectedItem.ToString() == string.Empty)
                {
                    if (cell != null)
                    {
                        cell.IsEnabled = false;
                    }
                }
                else
                {
                    if (cell != null)
                    {
                        cell.IsEnabled = true;
                    }
                }
            }

        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        } 

        public System.Windows.Controls.DataGridCell GetCell(int row, int column)
        {
            System.Windows.Controls.DataGridRow rowContainer = GetRow(row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // try to get the cell but it may possibly be virtualized
                System.Windows.Controls.DataGridCell cell = (System.Windows.Controls.DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // now try to bring into view and retreive the cell
                    DataGrid2.ScrollIntoView(rowContainer, DataGrid2.Columns[column]);
                    cell = (System.Windows.Controls.DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public System.Windows.Controls.DataGridRow GetRow(int index)
        {
            System.Windows.Controls.DataGridRow row = (System.Windows.Controls.DataGridRow)DataGrid2.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // may be virtualized, bring into view and try again
                DataGrid2.ScrollIntoView(DataGrid2.Items[index]);
                row = (System.Windows.Controls.DataGridRow)DataGrid2.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        private void DataGrid2_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            System.Windows.Controls.DataGridRow row = e.Row;
            TableColumnsMetadataItem item = (TableColumnsMetadataItem)row.Item;

            DataGrid2.SelectedItem = item;

            //DataGridCell cell = GetCell(DataGrid2.SelectedIndex, NamespaceColumn.DisplayIndex);

            //if (item.ColumnsSemantic == string.Empty)
            //{
            //    if (cell != null)
            //    {
            //        cell.IsEnabled = false;
            //    }
            //}
            //else
            //{
            //    if (cell != null)
            //    {
            //        cell.IsEnabled = true;
            //    }
            //}
        }
    }
}
