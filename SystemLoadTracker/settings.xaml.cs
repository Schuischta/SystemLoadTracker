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

namespace SystemLoadTracker
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }


        // UI event handlers for interaction
        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Background = Brushes.IndianRed;
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Background = Brushes.Transparent;
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        // Allows the window to be moved by dragging
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
