using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SystemLoadTracker
{
    public partial class Settings : Window
    {

        private MainWindow mainWindow;
        public Settings(double currentOpacity, MainWindow mainWindow)
        {
            InitializeComponent();
            opacitySlider.Value = currentOpacity;
            this.mainWindow = mainWindow;

            // Initialisieren Sie das Label basierend auf dem gespeicherten Zustand
            if (Properties.Settings.Default.FixedWindow)
            {
                FixedWindowCheckbox.Content = ""; // oder was auch immer der Standardinhalt ist
            }
            else
            {
                FixedWindowCheckbox.Content = "\uE73E"; // Das Icon
            }
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



        private void FixedWindowCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mainWindow.canMoveWindow) // Wenn das Fenster bereits bewegt werden kann
            {
                mainWindow.DisableWindowMove(); // Fensterbewegung deaktivieren
                FixedWindowCheckbox.Content = "\uE73E";
            }
            else
            {
                mainWindow.EnableWindowMove(); // Fensterbewegung aktivieren
                FixedWindowCheckbox.Content = "";
            }

            Properties.Settings.Default.FixedWindow = mainWindow.canMoveWindow;
            Properties.Settings.Default.Save();

        }

    }
}