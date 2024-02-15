using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SystemLoadTracker
{
    public partial class Settings : Window
    {
        public Settings(double currentOpacity)
        {
            InitializeComponent();
            opacitySlider.Value = currentOpacity;
        }

        // Changes the background color of the close button when the mouse enters
        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Background = Brushes.IndianRed;
        }

        // Resets the background color of the close button when the mouse leaves
        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Background = Brushes.Transparent;
        }

        // Closes the window when the close button is clicked
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

        // Changes the opacity of the main window when the slider value changes
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.SetOpacity(e.NewValue);
                Properties.Settings.Default.MainWindowOpacity = e.NewValue;
                Properties.Settings.Default.Save();
            }
        }
    }
}