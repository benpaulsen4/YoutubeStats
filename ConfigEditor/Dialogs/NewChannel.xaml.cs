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

        public NewChannel(Channel channel, HashSet<string> subGroups, ReactiveString selectedSubGroup)
        {
            this.channel = channel;
            this.subGroups = subGroups;
            this.selectedSubGroup = selectedSubGroup;
            this.InitializeComponent();
        }
    }
}
