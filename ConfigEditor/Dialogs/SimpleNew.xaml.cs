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
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SimpleNew : Page
    {
        private readonly ReactiveString name;
        private readonly HashSet<string> disallowedNames;
        private readonly ContentDialog dialogRef;

        public SimpleNew(ReactiveString name, HashSet<string> disallowedNames, ContentDialog dialogRef)
        {
            this.name = name;
            this.disallowedNames = disallowedNames;
            this.dialogRef = dialogRef;
            this.InitializeComponent();
            dialogRef.IsPrimaryButtonEnabled = false;
        }

        private void UpdateValidity(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                dialogRef.IsPrimaryButtonEnabled = false;
                validationError.Text = "Name cannot be blank";
                validationError.Visibility = Visibility.Visible;
            }
            else if (disallowedNames.Contains(textBox.Text))
            {
                dialogRef.IsPrimaryButtonEnabled = false;
                validationError.Text = $"The name '{textBox.Text}' is already in use";
                validationError.Visibility = Visibility.Visible;
            }
            else
            {
                dialogRef.IsPrimaryButtonEnabled = true;
                validationError.Visibility = Visibility.Collapsed;
            }
        }
    }
}
