using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#nullable enable

namespace Chess.UI.UI
{
    public partial class ComboboxItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Default Item Template for the items in the combobox
        /// </summary>
        public DataTemplate? ItemTemplate { get; set; }

        /// <summary>
        /// Temmplate used for the selected item when the combobox itself is closed and not expanded.
        /// </summary>
        public DataTemplate? SelectedItemContentTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            if (container is ComboBoxItem)
                return ItemTemplate;

            return SelectedItemContentTemplate;
        }
    }
}
