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
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor
{
    public sealed partial class GroupsPage : Page
    {
        public ObservableCollection<Group> Groups = new();

        private GeneralSettings generalSettings = new();

        public GroupsPage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();

            ShowMainTips();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateValidity();
            if (e.NavigationMode == NavigationMode.Back) ShowSaveTip();
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
            dialog.Content = new GeneralDialog(tempSettings, dialog);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                generalSettings = tempSettings;
                UpdateValidity();
            }

        }

        private async void ShowAddGroup(object sender, RoutedEventArgs e)
        {
            tip1.IsOpen = false;
            tip2.IsOpen = false;

            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Add Group";
            dialog.PrimaryButtonText = "Add";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var name = new ReactiveString("");
            var usedNames = Groups.Select(group => group.Name).ToHashSet();
            dialog.Content = new SimpleNew(name, usedNames, dialog);

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
            var groupName = (sender as FrameworkElement).DataContext as string;
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
            var groupName = (sender as FrameworkElement).DataContext as string;
            Frame.Navigate(typeof(EditGroupPage), Groups.First(group => group.Name == groupName));
        }

        private async void OnSave(object sender, RoutedEventArgs e)
        {
            // Create a file picker
            var savePicker = new FileSavePicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var window = (Application.Current as App).m_window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "config.json";

            var storageFile = await savePicker.PickSaveFileAsync();
            if (storageFile != null)
            {
                using var stream = await storageFile.OpenStreamForWriteAsync();
                await IO.SaveToFile(generalSettings, Groups, stream);
            }
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            tip1.IsOpen = false;
            tip2.IsOpen = false;

            // Create a file picker
            var openPicker = new FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var window = (Application.Current as App).m_window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.FileTypeFilter.Add(".json");

            var storageFile = await openPicker.PickSingleFileAsync();
            if (storageFile != null)
            {
                using var stream = await storageFile.OpenStreamForReadAsync();
                (generalSettings, Groups) = await IO.ReadFromFile(stream);
                list.ItemsSource = Groups;
                UpdateValidity();
            }
        }

        private void UpdateValidity()
        {
            saveButton.IsEnabled = generalSettings.ApiKey != null && generalSettings.ReportType != null && Groups.Any(group => group.SubGroups.Any(sub => sub.Channels.Any()));
            
        }

        private async void ShowMainTips()
        {
            //known bug in winui https://github.com/microsoft/microsoft-ui-xaml/issues/7937
            await Task.Delay(100);
            tip1.IsOpen = true;
            tip2.IsOpen = true;
        }

        private async void ShowSaveTip()
        {
            if (!saveButton.IsEnabled)
            {
                await Task.Delay(200);
                tip3.IsOpen = true;
            }
        }
    }
}
