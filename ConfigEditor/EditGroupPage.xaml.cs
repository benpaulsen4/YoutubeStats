using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ConfigEditor.Models;
using ConfigEditor.Dialogs;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditGroupPage : Page
    {
        private Group group;

        public EditGroupPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            group = e.Parameter as Group;
            base.OnNavigatedTo(e);
        }

        private async void ShowAddSub(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Add Sub Group";
            dialog.PrimaryButtonText = "Add";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var name = new ReactiveString("");
            dialog.Content = new NewGroup(name);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(name.Value))
            {
                group.SubGroups.Add(new SubGroup
                {
                    Name = name.Value,
                    Channels = new ObservableCollection<Channel>()
                });
            }
        }

        private async void ShowAddChannel(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Add Channel";
            dialog.PrimaryButtonText = "Add";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var channel = new Channel();
            var selected = new ReactiveString("");
         
            dialog.Content = new NewChannel(channel, group.SubGroups.Select(sub => sub.Name).ToHashSet(), selected);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(selected.Value))
            {
                group.SubGroups.First(sub => sub.Name ==  selected.Value).Channels.Add(channel);
            }
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }

    class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate SubTemplate { get; set; }
        public DataTemplate ChannelTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is SubGroup)
            {
                return SubTemplate;
            }
            else
            {
                return ChannelTemplate;
            }
        }
    }
}
