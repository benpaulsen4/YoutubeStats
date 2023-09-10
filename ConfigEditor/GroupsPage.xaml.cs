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
        public List<string> Items = new List<string>()
        {
            "test1",
            "test2",
            "test3"
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
            dialog.Content = new GeneralDialog(new GeneralSettings());

            var result = await dialog.ShowAsync();

        }

    }
}
