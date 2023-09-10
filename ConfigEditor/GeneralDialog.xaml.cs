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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor
{
    public sealed partial class GeneralDialog : Page
    {
        private readonly GeneralSettings generalSettings;
        private int selectedReportType = -1;

        //todo validate

        public GeneralDialog(GeneralSettings generalSettings)
        {
            this.InitializeComponent();
            this.generalSettings = generalSettings;

            if (generalSettings.ReportType != null)
            {
                switch (generalSettings.ReportType)
                {
                    case "console":
                        selectedReportType = 0;
                        break;
                    case "csv":
                        selectedReportType = 1;
                        break;
                    case "analytics":
                        selectedReportType = 2;
                        break;
                }
            }
        }

        private void ShowKeyChanged(object sender, RoutedEventArgs e)
        {
            if (revealKey.IsChecked == true)
            {
                apiKey.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                apiKey.PasswordRevealMode = PasswordRevealMode.Hidden;
            }

        }

        private void ReportChanged(object sender, RoutedEventArgs e)
        {
            generalSettings.ReportType = (sender as RadioButton).Content.ToString().ToLower();
        }

        private void KeyChanged(object sender, RoutedEventArgs e)
        {
            generalSettings.ApiKey = apiKey.Password;
        }
    }
}
