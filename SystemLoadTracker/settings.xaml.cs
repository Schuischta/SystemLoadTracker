using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SystemLoadTracker
{
    public partial class Settings : Window
    {

        private MainWindow mainWindow;
        private Color colorON;
        private Color colorOFF;
        public Settings(double currentOpacity, MainWindow mainWindow)
        {
            InitializeComponent();
            opacitySlider.Value = currentOpacity;
            this.mainWindow = mainWindow;


            colorON = (Color)ColorConverter.ConvertFromString("#FF5F5F5F");
            colorOFF = (Color)ColorConverter.ConvertFromString("#FFB0B0B0");


            // Initialisieren Sie das Label basierend auf dem gespeicherten Zustand
            if (Properties.Settings.Default.FixedWindow)
            {
                FixedWindowCheckbox.Content = ""; // oder was auch immer der Standardinhalt ist
            }
            else
            {
                FixedWindowCheckbox.Content = "\uE73E"; // Das Icon
            }


            // Initialize the label based on the saved setting
            AlwaysOnTopCheckbox.Content = Properties.Settings.Default.AlwaysOnTop ? "\uE73E" : "";


            // Initialisieren Sie die Labels basierend auf der gespeicherten Einstellung
            double currentInterval = Properties.Settings.Default.RefreshInterval;
            RefreshTimeCheckbox05.Background = currentInterval == 0.5 ? Brushes.White : Brushes.Transparent;
            RefreshTimeCheckbox05.Foreground = currentInterval == 0.5 ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);
            RefreshTimeCheckbox1.Background = currentInterval == 1.0 ? Brushes.White : Brushes.Transparent;
            RefreshTimeCheckbox1.Foreground = currentInterval == 1.0 ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);
            RefreshTimeCheckbox2.Background = currentInterval == 2.0 ? Brushes.White : Brushes.Transparent;
            RefreshTimeCheckbox2.Foreground = currentInterval == 2.0 ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);
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


        // Toggles the Always on Top setting
        private void AlwaysOnTopCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.ToggleAlwaysOnTop(); // Ändere den Zustand von Always on Top

            // Aktualisiere den Inhalt des Labels, um den aktuellen Zustand widerzuspiegeln
            AlwaysOnTopCheckbox.Content = Properties.Settings.Default.AlwaysOnTop ? "\uE73E" : "";
        }




        private void RefreshTimeCheckbox05_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetRefreshInterval(0.5);
        }

        private void RefreshTimeCheckbox1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetRefreshInterval(1.0);
        }

        private void RefreshTimeCheckbox2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetRefreshInterval(2.0);
        }

        private void SetRefreshInterval(double interval)
        {
            Properties.Settings.Default.RefreshInterval = interval;
            Properties.Settings.Default.Save();

            mainWindow.UpdateTimerInterval(interval); // Methode im MainWindow, um das Timer-Intervall zu aktualisieren

            // Aktualisieren Sie die Inhalte der Labels, um die Auswahl widerzuspiegeln

            RefreshTimeCheckbox05.Background = interval == 0.5 ? Brushes.White : Brushes.Transparent;
            RefreshTimeCheckbox05.Foreground = interval == 0.5 ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);
            RefreshTimeCheckbox1.Background = interval == 1.0 ? Brushes.White : Brushes.Transparent;
            RefreshTimeCheckbox1.Foreground = interval == 1.0 ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);
            RefreshTimeCheckbox2.Background = interval == 2.0 ? Brushes.White : Brushes.Transparent;
            RefreshTimeCheckbox2.Foreground = interval == 2.0 ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);

        }





    }
}