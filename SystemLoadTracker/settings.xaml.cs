using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SystemLoadTracker
{
    public partial class Settings : Window
    {
        private const string ColorOnCode = "#FF5F5F5F";
        private const string ColorOffCode = "#FFB0B0B0";
        private const string IconCode = "\uE73E";

        private MainWindow mainWindow;
        private Color colorON;
        private Color colorOFF;

        public Settings(double currentOpacity, double currentCornerRadius, MainWindow mainWindow)
        {
            InitializeComponent();
            opacitySlider.Value = currentOpacity;
            cornerRadiusSlider.Value = currentCornerRadius;
            this.mainWindow = mainWindow;

            colorON = (Color)ColorConverter.ConvertFromString(ColorOnCode);
            colorOFF = (Color)ColorConverter.ConvertFromString(ColorOffCode);

            settingsWindowCorner.CornerRadius = new CornerRadius(Properties.Settings.Default.SettingsWindowCornerRadius);
            closeButtonBorder.CornerRadius = new CornerRadius(0, Properties.Settings.Default.SettingsWindowCornerRadius, 0, 0);

            Loaded += (s, e) => InitializeControls();
        }


        private void InitializeControls()
        {
            FixedWindowCheckbox.Content = Properties.Settings.Default.FixedWindow ? "" : IconCode;
            AlwaysOnTopCheckbox.Content = Properties.Settings.Default.AlwaysOnTop ? IconCode : "";
            showBorderCheckbox.Content = Properties.Settings.Default.ShowMainWindowBorder ? IconCode : "";
            cornerRadiusSlider.Value = Properties.Settings.Default.MainWindowCornerRadius;

            double currentInterval = Properties.Settings.Default.RefreshInterval;
            SetCheckboxColors(RefreshTimeCheckbox05, currentInterval, 0.5);
            SetCheckboxColors(RefreshTimeCheckbox1, currentInterval, 1.0);
            SetCheckboxColors(RefreshTimeCheckbox2, currentInterval, 2.0);
        }

        private void SetCheckboxColors(Control checkbox, double currentInterval, double targetInterval)
        {
            checkbox.Background = currentInterval == targetInterval ? Brushes.White : Brushes.Transparent;
            checkbox.Foreground = currentInterval == targetInterval ? new SolidColorBrush(colorON) : new SolidColorBrush(colorOFF);
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButtonBorder.Background = Brushes.IndianRed;
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButtonBorder.Background = Brushes.Transparent;
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

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
            if (mainWindow.canMoveWindow)
            {
                mainWindow.DisableWindowMove();
            }
            else
            {
                mainWindow.EnableWindowMove();
            }

            Properties.Settings.Default.FixedWindow = mainWindow.canMoveWindow;
            Properties.Settings.Default.Save();

            FixedWindowCheckbox.Content = mainWindow.canMoveWindow ? "" : IconCode;
        }

        private void AlwaysOnTopCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.ToggleAlwaysOnTop();

            AlwaysOnTopCheckbox.Content = Properties.Settings.Default.AlwaysOnTop ? IconCode : "";
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

            mainWindow.UpdateTimerInterval(interval);

            SetCheckboxColors(RefreshTimeCheckbox05, interval, 0.5);
            SetCheckboxColors(RefreshTimeCheckbox1, interval, 1.0);
            SetCheckboxColors(RefreshTimeCheckbox2, interval, 2.0);
        }

        private void showBorderCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var showBorder = !Properties.Settings.Default.ShowMainWindowBorder;
            mainWindow.ToggleMainWindowBorder(showBorder);

            showBorderCheckbox.Content = showBorder ? IconCode : "";
        }

        private void cornerRadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newCornerRadius = e.NewValue;
            if (mainWindow != null)
            {
                mainWindow.SetCornerRadius(newCornerRadius);
                settingsWindowCorner.CornerRadius = new CornerRadius(newCornerRadius);
                closeButtonBorder.CornerRadius = new CornerRadius(0, newCornerRadius, 0, 0);
            }
            Properties.Settings.Default.SettingsWindowCornerRadius = newCornerRadius;
            Properties.Settings.Default.MainWindowCornerRadius = newCornerRadius;
            Properties.Settings.Default.Save();
        }
    }
}
