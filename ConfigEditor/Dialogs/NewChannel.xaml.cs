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
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ConfigEditor.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewChannel : Page
    {
        private readonly Channel channel;
        private readonly HashSet<string> subGroups;
        private readonly ReactiveString selectedSubGroup;
        private readonly bool canChangeSubGroup;
        private readonly HashSet<string> disallowedNames;
        private readonly ContentDialog dialogRef;
        private readonly Regex channelIdRegex = new("^@.+|UC.{22}$");

        private bool nameValid = false;
        private bool idValid = false;
        private bool subGroupValid = false;

        public NewChannel(Channel channel, HashSet<string> subGroups, ReactiveString selectedSubGroup, bool canChangeSubGroup, HashSet<string> disallowedNames, ContentDialog dialogRef)
        {
            this.channel = channel;
            this.channel.IgnoreInAverage ??= false;
            this.subGroups = subGroups;
            this.selectedSubGroup = selectedSubGroup;
            this.canChangeSubGroup = canChangeSubGroup;
            this.disallowedNames = disallowedNames;
            this.dialogRef = dialogRef;
            this.InitializeComponent();

            dialogRef.IsPrimaryButtonEnabled = false;
            if (!string.IsNullOrEmpty(channel.Id) || !string.IsNullOrEmpty(channel.Name)) UpdateValidity(null, null);
        }

        private void UpdateValidity(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox { Name: "name" } or null)
            {
                if (string.IsNullOrWhiteSpace(name.Text))
                {
                    nameError.Text = "Name cannot be blank";
                    nameError.Visibility = Visibility.Visible;
                    nameValid = false;
                }
                else if (disallowedNames.Contains(name.Text))
                {
                    nameError.Text = $"The name '{name.Text}' is already in use";
                    nameError.Visibility = Visibility.Visible;
                    nameValid = false;
                }
                else
                {
                    nameError.Visibility = Visibility.Collapsed;
                    nameValid = true;
                }
            }

            if (sender is TextBox { Name: "id" } or null)
            {
                if (string.IsNullOrWhiteSpace(id.Text))
                {
                    idError.Text = "Channel ID cannot be blank";
                    idError.Visibility = Visibility.Visible;
                    idValid = false;
                }
                else if (!channelIdRegex.IsMatch(id.Text))
                {
                    idError.Text = "Channel ID must be a Youtube handle starting with '@' or a valid Youtube ID";
                    idError.Visibility = Visibility.Visible;
                    idValid = false;
                }
                else
                {
                    idError.Visibility = Visibility.Collapsed;
                    idValid = true;
                }
            }

            if (sender is ComboBox or null)
            {
                if (subGroup.SelectedValue == null)
                {
                    subError.Visibility = Visibility.Visible;
                    subGroupValid = false;
                }
                else 
                {
                    subError.Visibility = Visibility.Collapsed;
                    subGroupValid = true;
                }
            }

            dialogRef.IsPrimaryButtonEnabled = nameValid && idValid && subGroupValid;
        }
    }
}
