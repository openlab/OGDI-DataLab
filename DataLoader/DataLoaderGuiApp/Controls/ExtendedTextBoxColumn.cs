using System;
using System.Windows;
using System.Windows.Controls;
using DataGridCell = System.Windows.Controls.DataGridCell;
using DataGridTextColumn = System.Windows.Controls.DataGridTextColumn;

namespace Ogdi.Data.DataLoaderGuiApp.Controls
{
    public class ExtendedTextBoxColumn : DataGridTextColumn
    {
        #region Properties

        #region HorizontalAlignment Property

        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register(
                "HorizontalAlignment",
                typeof(HorizontalAlignment),
                typeof(ExtendedTextBoxColumn),
                new UIPropertyMetadata(HorizontalAlignment.Stretch));

        #endregion

        #region VerticalAlignment Property

        public VerticalAlignment VerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalAlignmentProperty); }
            set { SetValue(VerticalAlignmentProperty, value); }
        }

        public static readonly DependencyProperty VerticalAlignmentProperty =
            DependencyProperty.Register(
                "VerticalAlignment",
                typeof(VerticalAlignment),
                typeof(ExtendedTextBoxColumn),
                new UIPropertyMetadata(VerticalAlignment.Stretch));

        #endregion

        #endregion

        #region Implementation

        private TextAlignment GetTextAlignment()
        {
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    return TextAlignment.Center;
                case HorizontalAlignment.Left:
                    return TextAlignment.Left;
                case HorizontalAlignment.Right:
                    return TextAlignment.Right;
                case HorizontalAlignment.Stretch:
                    return TextAlignment.Justify;
                default:
                    throw new ArgumentOutOfRangeException("HorizontalAlignment", "Unsupported alignment type!");
            }
        }

        #endregion

        #region DataGridTextColumn overrides

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var element = base.GenerateElement(cell, dataItem);

            element.HorizontalAlignment = HorizontalAlignment;
            element.VerticalAlignment = VerticalAlignment;

            return element;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var textBox = (TextBox)base.GenerateEditingElement(cell, dataItem);

            textBox.TextAlignment = GetTextAlignment();
            textBox.VerticalContentAlignment = VerticalAlignment;

            return textBox;
        }

        #endregion
    }
}
