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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditGroupPage : Page
    {
        private Models.Group group;

        public EditGroupPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            group = e.Parameter as Models.Group;
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
            var usedNames = group.SubGroups.Select(sub => sub.Name).ToHashSet();
            dialog.Content = new SimpleNew(name, usedNames, dialog);

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
            var namesInUse = group.SubGroups.SelectMany(sub => sub.Channels).Select(channel => channel.Name).ToHashSet();
         
            dialog.Content = new NewChannel(channel, group.SubGroups.Select(sub => sub.Name).ToHashSet(), selected, true, namesInUse, dialog);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(selected.Value))
            {
                group.SubGroups.First(sub => sub.Name ==  selected.Value).Channels.Add(channel);
            }
        }

        private async void OnEditChannel(object sender, RoutedEventArgs e)
        {
            var channelName = (sender as FrameworkElement).DataContext as string;
            ContentDialog dialog = new ContentDialog();

            var (sub, channel) = GetSubGroupAndChannel(channelName);

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Edit Channel";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var newChannel = new Channel()
            {
                Name = channel.Name,
                Id = channel.Id,
                IgnoreInAverage = channel.IgnoreInAverage
            };
            var selected = new ReactiveString(sub.Name);
            var namesInUse = group.SubGroups.SelectMany(sub => sub.Channels).Select(channel => channel.Name).ToHashSet();
            namesInUse.Remove(channelName);

            dialog.Content = new NewChannel(newChannel, group.SubGroups.Select(sub => sub.Name).ToHashSet(), selected, false, namesInUse, dialog);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (newChannel.Name != channel.Name) 
                {
                    //need to hard reset the channel
                    sub.Channels.Remove(channel);
                    sub.Channels.Add(newChannel);

                    dialog.Title = "Channel Name";
                    dialog.CloseButtonText = "Ok";
                    dialog.PrimaryButtonText = null;
                    dialog.Content = "Editing a channel's name will break existing data that has been collected. Make sure to also change the name of the channel in the CSV header for this subgroup.";
                    await dialog.ShowAsync();
                } 
                else
                {
                    channel.Id = newChannel.Id;
                    channel.IgnoreInAverage = newChannel.IgnoreInAverage;
                }
            }
        }

        private async void OnDeleteChannel(object sender, RoutedEventArgs e)
        {
            var channelName = (sender as FrameworkElement).DataContext as string;
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = $"Delete {channelName}?";
            dialog.PrimaryButtonText = "Delete";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "Are you sure you would like to delete this creator? This action cannot be undone.";

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var (sub, channel) = GetSubGroupAndChannel(channelName);
                sub.Channels.Remove(channel);
            }
        }

        private async void OnDeleteSubGroup(object sender, RoutedEventArgs e)
        {
            var groupName = (sender as FrameworkElement).DataContext as string;
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = $"Delete {groupName}?";
            dialog.PrimaryButtonText = "Delete";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "Are you sure you would like to delete this sub group? This action cannot be undone.";

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                group.SubGroups.Remove(group.SubGroups.First(group => group.Name == groupName));
            }
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private (SubGroup sub, Channel channel) GetSubGroupAndChannel(string channelName)
        {
            foreach(var sub in group.SubGroups)
            {
                var result = sub.Channels.FirstOrDefault(ch => ch.Name == channelName);
                if (result != null) return (sub, result);
            }
            return (null, null);
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
