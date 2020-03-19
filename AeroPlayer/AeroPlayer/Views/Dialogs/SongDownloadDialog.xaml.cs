using AeroPlayer.ViewModels;
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
    /// Interaction logic for SongDownloadDialog.xaml
    /// </summary>
    public partial class SongDownloadDialog : Window
    {
        public SongDownloadDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((SongDownloadDialogViewModel)DataContext).LoadPlaylists();
        }
    }
}
