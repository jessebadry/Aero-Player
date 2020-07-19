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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ErrorDialog : Window
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }
        public ErrorDialog(string msg) : base()
        {
            InitializeComponent();
            this.ErrorText.Text = msg;
        }
    }
}
