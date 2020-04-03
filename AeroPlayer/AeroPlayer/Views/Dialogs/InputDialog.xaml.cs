using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AeroPlayer.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {

        public InputDialog(string message)
        {
            InitializeComponent();
            this.MessageBlock.Text = message;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application current = Application.Current;
            Window mainWin = current.MainWindow;
            this.Left = mainWin.Left + (mainWin.Width - this.ActualWidth) / 2;
            this.Top = mainWin.Top + (mainWin.Height - this.ActualHeight) / 2;

        }
    }
}
