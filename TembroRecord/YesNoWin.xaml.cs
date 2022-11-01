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

namespace TembroRecord
{
    /// <summary>
    /// Логика взаимодействия для YesNoWin.xaml
    /// </summary>
    public partial class YesNoWin : Window
    {
        public static int btnOKInd = 0;

        public YesNoWin()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WinOK_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            btnOKInd = 1;
        }
    }
}
