using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ConfigEditor.Models;

namespace ConfigEditor.Dialogs;

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