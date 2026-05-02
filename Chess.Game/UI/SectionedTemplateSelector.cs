using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#nullable enable

namespace Chess.UI.UI
{
    public partial class ComboboxItemContainerStyleSelector : StyleSelector
    {
        public Style? EnabledItemStyle { get; set; }
        public Style? DisabledItemStyle { get; set; }

        protected override Style? SelectStyleCore(object item, DependencyObject container)
        {
            if (item is IComboNode node)
            {
                // Placeholder items should be disabled
                return node.IsPlaceHolderItem ? DisabledItemStyle : EnabledItemStyle;
            }

            // Default to enabled item style
            return EnabledItemStyle;
        }
    }

    public sealed partial class SectionedTemplateSelector : DataTemplateSelector
    {
        public DataTemplateSelector? ItemTemplateSelector { get; set; }

        public DataTemplate? ItemTemplate { get; set; }
        public DataTemplate? DividerTemplate { get; set; }
        public DataTemplate? HeaderTemplate { get; set; }

        private DataTemplate? Select(object item, DependencyObject? container)
        {
            if (item is IComboNode node)
            {
                return node.Kind switch
                {
                    ComboNodeKind.Header => HeaderTemplate,
                    ComboNodeKind.Divider => DividerTemplate,
                    ComboNodeKind.Item => ItemTemplateSelector?.SelectTemplate(item, container) ?? ItemTemplate,
                    _ => ItemTemplateSelector?.SelectTemplate(item, container) ?? ItemTemplate,
                };
            }
            return ItemTemplateSelector?.SelectTemplate(item, container) ?? ItemTemplate;
        }

        protected override DataTemplate? SelectTemplateCore(object item) => Select(item, null);

        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => Select(item, container);
    }
}

