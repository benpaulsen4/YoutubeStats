using ConfigEditor.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ConfigEditor;

public sealed partial class GeneralDialog : Page
{
  private readonly GeneralSettings generalSettings;
  private readonly ContentDialog dialogRef;
  private int selectedReportType = -1;

  public GeneralDialog(GeneralSettings generalSettings, ContentDialog dialogRef)
  {
    this.InitializeComponent();
    this.generalSettings = generalSettings;
    this.dialogRef = dialogRef;
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

    UpdateValidity(null, null);
  }

  private void ShowKeyChanged(object sender, RoutedEventArgs e)
  {
    apiKey.PasswordRevealMode = revealKey.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
  }

  private void UpdateValidity(object sender, RoutedEventArgs e)
  {
    if (string.IsNullOrWhiteSpace(apiKey.Password))
    {
      validationError.Visibility = Visibility.Visible;
      dialogRef.IsPrimaryButtonEnabled = false;
    }
    else if (generalSettings.ReportType == null)
    {
      validationError.Visibility = Visibility.Collapsed;
      dialogRef.IsPrimaryButtonEnabled = false;
    }
    else
    {
      validationError.Visibility = Visibility.Collapsed;
      dialogRef.IsPrimaryButtonEnabled = true;
    }
  }

  private void ReportChanged(object sender, RoutedEventArgs e)
  {
    generalSettings.ReportType = (sender as RadioButton).Content.ToString().ToLower();

    UpdateValidity(null, null);
  }
}