using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EBSOrdersManager
{
    /// <summary>
    /// Logica di interazione per StringInputDialogBox.xaml
    /// </summary>
    public partial class StringInputDialogBox : Window
    {
        public string output = "";

        public StringInputDialogBox(string title, bool showCancel = true)
        {
            InitializeComponent();
            Title.Text = title;

            if (!showCancel)
                btnCancel.Visibility = Visibility.Collapsed;
            
            Input.Focus();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Confirm_Click(sender, e);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            output = Input.Text;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
