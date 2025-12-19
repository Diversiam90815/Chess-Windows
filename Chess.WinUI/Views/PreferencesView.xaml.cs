using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

#nullable enable

namespace Chess.UI.Views
{
    public sealed partial class PreferencesView : ContentDialog
    {
        private readonly Dictionary<string, Type> _views = new();

        private readonly Dictionary<string, object> _viewmodels = new();

        private readonly StylesPreferencesViewModel _settingsViewModel;

        public event Action? ButtonClicked;


        public PreferencesView()
        {
            this.InitializeComponent();

            _settingsViewModel = App.Current.Services.GetRequiredService<StylesPreferencesViewModel>();
        }


        public void AddPreferencesTab(string name, Type viewType, string? fontIconGlyph = null)
        {
            _views[name] = viewType;

            var item = new NavigationViewItem()
            {
                Content = name,
                Tag = name,
            };

            if (!string.IsNullOrEmpty(fontIconGlyph))
                item.Icon = new FontIcon() { Glyph = fontIconGlyph };

            NavigationView.MenuItems.Add(item);

            // If this is the first item, select it
            if (NavigationView.MenuItems.Count == 1)
                NavigationView.SelectedItem = item;
        }


        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer?.Tag is string viewTag && _views.TryGetValue(viewTag, out var viewType))
            {
                object? viewModel;
                if (_viewmodels.TryGetValue(viewTag, out viewModel) == false)
                {
                    viewModel = _settingsViewModel;
                }
                NavigationViewFrame.Navigate(viewType, viewModel);

                ButtonClicked?.Invoke();
            }
        }


        private void ReturnButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonClicked?.Invoke();
            this.Hide();
        }
    }
}
