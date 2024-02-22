using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32.TaskScheduler;

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
            ChangeSettingsTheme();
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
            PCStartupCheckbox.Content = Properties.Settings.Default.StartWithWindows ? IconCode : "";
            SystemTrayCheckbox.Content = Properties.Settings.Default.ShowInSystemTray ? IconCode : "";

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


        private void StartupCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Properties.Settings.Default.StartWithWindows)
            {
                DeleteStartupTask();
                PCStartupCheckbox.Content = "";
                Properties.Settings.Default.StartWithWindows = false;
            }
            else
            {
                CreateStartupTask();
                PCStartupCheckbox.Content = IconCode;
                Properties.Settings.Default.StartWithWindows = true;
            }

            Properties.Settings.Default.Save();
        }


        public void CreateStartupTask()
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "SystemLoadTracker";
                td.Principal.RunLevel = TaskRunLevel.Highest; // Run with highest privileges

                td.Triggers.Add(new LogonTrigger { Enabled = true }); // Trigger on logon

                string appDirectory = System.AppContext.BaseDirectory;
                td.Actions.Add(new ExecAction(Path.Combine(appDirectory, "SystemLoadTracker.exe"), null, appDirectory)); // Action to start the application

                ts.RootFolder.RegisterTaskDefinition("SystemLoadTracker", td); // Register the task
            }
        }

        public void DeleteStartupTask()
        {
            using (TaskService ts = new TaskService())
            {
                ts.RootFolder.DeleteTask("SystemLoadTracker");
            }
        }


        private void SystemTrayCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool showInSystemTray = !Properties.Settings.Default.ShowInSystemTray;
            Properties.Settings.Default.ShowInSystemTray = showInSystemTray;
            Properties.Settings.Default.Save();

            mainWindow.SetNotifyIconVisibility(showInSystemTray);
            mainWindow.ShowInTaskbar = !showInSystemTray;

            SystemTrayCheckbox.Content = showInSystemTray ? IconCode : "";
        }



        // Change the theme
        public void ChangeSettingsTheme()
        {
            var theme = Properties.Settings.Default.Theme;
            var dict = new ResourceDictionary();

            if (theme == "Dark")
            {
                dict.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }
            else
            {
                dict.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            }

            this.Resources.MergedDictionaries.Add(dict);
        }



        private void LightThemeLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.Theme = "Light";
            Properties.Settings.Default.Save();
            
            ChangeSettingsTheme();
            mainWindow.ChangeTheme();
        }

        private void DarkThemeLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.Theme = "Dark";
            Properties.Settings.Default.Save();

            ChangeSettingsTheme();
            mainWindow.ChangeTheme();
        }


    }
}
