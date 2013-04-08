using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ogdi.Data.DataLoaderGuiApp.Controls
{
    public class LabelTextBoxColumn : ExtendedTextBoxColumn
    {
        #region Implementation

        private void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing, bool defaultToElementStyle)
        {
            Style style = isEditing ? EditingElementStyle : ElementStyle;
            if (isEditing && defaultToElementStyle && (style == null))
            {
                style = ElementStyle;
            }
            
            return style;
        }

        private void ApplyBinding(DependencyObject target, DependencyProperty property)
        {
            BindingBase binding = Binding;
            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);
            }
            else
            {
                BindingOperations.ClearBinding(target, property);
            }
        }

        #endregion

        #region DataGridTextBoxColumn overrides

        protected override FrameworkElement GenerateElement(System.Windows.Controls.DataGridCell cell, object dataItem)
        {
            var label = new Label
                            {
                                HorizontalAlignment = HorizontalAlignment,
                                VerticalAlignment = VerticalAlignment
                            };

            ApplyStyle(false, false, label);
            ApplyBinding(label, ContentControl.ContentProperty);

            return label;
        }

        #endregion
    }
}