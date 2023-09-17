using ConfigEditor.Dialogs;
using ConfigEditor.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor
{
    public sealed partial class GroupsPage : Page
    {
        public ObservableCollection<Group> Groups = new ObservableCollection<Group>()
        {
            new Group()
            {
                Name = "Example A",
                SubGroups = new ObservableCollection<SubGroup>()
                {
                    new SubGroup
                    {
                        Name = "SubA",
                        Channels = new ObservableCollection<Channel>()
                        {
                            new Channel()
                            {
                                Name="Test",
                                Id="int"
                            }
                        }
                    }
                }
            },
            new Group()
            {
                Name = "Example B",
                SubGroups = new ObservableCollection<SubGroup>()
                {
                    new SubGroup
                    {
                        Name = "SubB",
                        Channels = new ObservableCollection<Channel>()
                    }
                }
            }
        };

        private GeneralSettings generalSettings = new();

        public GroupsPage()
        {
            this.InitializeComponent();
        }

        private async void ShowGeneral(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "General Settings";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var tempSettings = new GeneralSettings()
            {
                ApiKey = generalSettings.ApiKey,
                ReportType = generalSettings.ReportType,
            };
            dialog.Content = new GeneralDialog(tempSettings);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                generalSettings = tempSettings;
            }

        }

        private async void ShowAddGroup(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Add Group";
            dialog.PrimaryButtonText = "Add";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var name = new ReactiveString("");
            dialog.Content = new NewGroup(name);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(name.Value))
            {
                Groups.Add(new Group
                {
                    Name = name.Value,
                    SubGroups = new ObservableCollection<SubGroup>()
                }) ;
            }
        }

        private async void OnDeleteGroup(object sender, RoutedEventArgs e)
        {
            var groupName = (sender as MenuFlyoutItem).DataContext as string;
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = $"Delete {groupName}?";
            dialog.PrimaryButtonText = "Delete";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "Are you sure you would like to delete this group? This action cannot be undone.";

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Groups.Remove(Groups.First(group => group.Name == groupName));
            }
        }

        private void OnEditGroup(object sender, RoutedEventArgs e)
        {
            var groupName = (sender as MenuFlyoutItem).DataContext as string;
            Frame.Navigate(typeof(EditGroupPage), Groups.First(group => group.Name == groupName));
        }
    }
}
