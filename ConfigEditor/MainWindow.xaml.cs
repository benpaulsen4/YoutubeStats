using Microsoft.UI.Xaml;

namespace ConfigEditor;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    this.InitializeComponent();
    this.AppWindow.Resize(new(700, 800));
  }
}