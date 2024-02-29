using Microsoft.Win32.TaskScheduler;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SystemLoadTracker
{
    public partial class Settings : Window
    {
        private const string IconCode = "\uE73E";

        private MainWindow mainWindow;

        public Settings(double currentOpacity, double currentCornerRadius, MainWindow mainWindow)
        {
            InitializeComponent();
            ChangeSettingsTheme();
            opacitySlider.Value = currentOpacity;
            cornerRadiusSlider.Value = currentCornerRadius;
            this.mainWindow = mainWindow;

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
            SetRefreshTimeButtonColors(RefreshTimeCheckbox05, currentInterval, 0.5);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox1, currentInterval, 1.0);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox2, currentInterval, 2.0);

            SetThemeButtonColors(LightThemeLabel, Properties.Settings.Default.Theme, "Light");
            SetThemeButtonColors(DarkThemeLabel, Properties.Settings.Default.Theme, "Dark");
        }

        private void SetRefreshTimeButtonColors(Control checkbox, double currentInterval, double targetInterval)
        {
            if (currentInterval == targetInterval)
            {
                checkbox.Background = (Brush)FindResource("ButtonBackgroundColorON");
                checkbox.Foreground = (Brush)FindResource("ButtonTextColorON");
            }
            else
            {
                checkbox.Background = (Brush)FindResource("ButtonBackgroundColorOFF");
                checkbox.Foreground = (Brush)FindResource("ButtonTextColorOFF");
            }
        }


        private void SetThemeButtonColors(Label label, string currentTheme, string targetTheme)
        {
            if (currentTheme == targetTheme)
            {
                label.Background = (Brush)FindResource("ButtonBackgroundColorON");
                label.Foreground = (Brush)FindResource("ButtonTextColorON");
            }
            else
            {
                label.Background = (Brush)FindResource("ButtonBackgroundColorOFF");
                label.Foreground = (Brush)FindResource("ButtonTextColorOFF");
            }
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

            SetRefreshTimeButtonColors(RefreshTimeCheckbox05, interval, 0.5);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox1, interval, 1.0);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox2, interval, 2.0);
        }

        private void showBorderCheckbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var showBorder = !Properties.Settings.Default.ShowMainWindowBorder;
            var cornerRadius = Properties.Settings.Default.MainWindowCornerRadius;
            mainWindow.ToggleMainWindowBorder(showBorder, cornerRadius);

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

                if (mainWindow.mainWindowBorder.Visibility == Visibility.Visible)
                {
                    mainWindow.mainWindowCorner.CornerRadius = new CornerRadius(newCornerRadius + 4);
                }
                else
                {
                    mainWindow.mainWindowCorner.CornerRadius = new CornerRadius(newCornerRadius);
                }
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

            SetRefreshTimeButtonColors(RefreshTimeCheckbox05, Properties.Settings.Default.RefreshInterval, 0.5);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox1, Properties.Settings.Default.RefreshInterval, 1.0);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox2, Properties.Settings.Default.RefreshInterval, 2.0);

            SetThemeButtonColors(LightThemeLabel, Properties.Settings.Default.Theme, "Light");
            SetThemeButtonColors(DarkThemeLabel, Properties.Settings.Default.Theme, "Dark");            
        }

        private void DarkThemeLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.Theme = "Dark";
            Properties.Settings.Default.Save();

            ChangeSettingsTheme();
            mainWindow.ChangeTheme();

            SetRefreshTimeButtonColors(RefreshTimeCheckbox05, Properties.Settings.Default.RefreshInterval, 0.5);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox1, Properties.Settings.Default.RefreshInterval, 1.0);
            SetRefreshTimeButtonColors(RefreshTimeCheckbox2, Properties.Settings.Default.RefreshInterval, 2.0);

            SetThemeButtonColors(LightThemeLabel, Properties.Settings.Default.Theme, "Light");
            SetThemeButtonColors(DarkThemeLabel, Properties.Settings.Default.Theme, "Dark");
        }


        // Change the color of the progress bars
        private void CpuColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            // Laden Sie die zuletzt ausgewählte Farbe
            System.Drawing.Color lastColor = Properties.Settings.Default.CpuColor;
            colorDialog.Color = lastColor;

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color winFormsColor = colorDialog.Color;
                Color wpfColor = Color.FromArgb(winFormsColor.A, winFormsColor.R, winFormsColor.G, winFormsColor.B);

                mainWindow.progressbarCPU.Foreground = new SolidColorBrush(wpfColor);
                mainWindow.progressbarCPUtemp.Foreground = new SolidColorBrush(wpfColor);
                mainWindow.progressbarRAM.Foreground = new SolidColorBrush(wpfColor);

                var gradientBrush = mainWindow.mainWindowBorder.BorderBrush as GradientBrush;
                if (gradientBrush != null && gradientBrush.GradientStops.Count > 1)
                {
                    gradientBrush.GradientStops[1].Color = wpfColor;
                }

                System.Drawing.Color colorToSave = System.Drawing.Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

                // Speichern Sie die ausgewählte Farbe
                Properties.Settings.Default.CpuColor = colorToSave;
                Properties.Settings.Default.Save();
            }
        }

        private void GpuColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            // Laden Sie die zuletzt ausgewählte Farbe
            System.Drawing.Color lastColor = Properties.Settings.Default.GpuColor;
            colorDialog.Color = lastColor;

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color winFormsColor = colorDialog.Color;
                Color wpfColor = Color.FromArgb(winFormsColor.A, winFormsColor.R, winFormsColor.G, winFormsColor.B);

                mainWindow.progressbarGPU.Foreground = new SolidColorBrush(wpfColor);
                mainWindow.progressbarGPUtemp.Foreground = new SolidColorBrush(wpfColor);
                mainWindow.progressbarVRAM.Foreground = new SolidColorBrush(wpfColor);

                var gradientBrush = mainWindow.mainWindowBorder.BorderBrush as GradientBrush;
                if (gradientBrush != null && gradientBrush.GradientStops.Count > 0)
                {
                    gradientBrush.GradientStops[0].Color = wpfColor;
                }

                System.Drawing.Color colorToSave = System.Drawing.Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

                // Speichern Sie die ausgewählte Farbe
                Properties.Settings.Default.GpuColor = colorToSave;
                Properties.Settings.Default.Save();
            }
        }



        // Reset the color of the progress bars
        private void ResetCpuColorButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Color defaultCpuColor = Color.FromRgb(92, 180, 190);

            mainWindow.progressbarCPU.Foreground = new SolidColorBrush(defaultCpuColor);
            mainWindow.progressbarCPUtemp.Foreground = new SolidColorBrush(defaultCpuColor);
            mainWindow.progressbarRAM.Foreground = new SolidColorBrush(defaultCpuColor);

            var gradientBrush = mainWindow.mainWindowBorder.BorderBrush as GradientBrush;
            if (gradientBrush != null && gradientBrush.GradientStops.Count > 1)
            {
                gradientBrush.GradientStops[1].Color = defaultCpuColor;
            }

            Properties.Settings.Default.CpuColor = System.Drawing.Color.FromArgb(defaultCpuColor.A, defaultCpuColor.R, defaultCpuColor.G, defaultCpuColor.B);
            Properties.Settings.Default.Save();
        }

        private void ResetGpuColorButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Color defaultGpuColor = Color.FromRgb(61, 181, 107);

            mainWindow.progressbarGPU.Foreground = new SolidColorBrush(defaultGpuColor);
            mainWindow.progressbarGPUtemp.Foreground = new SolidColorBrush(defaultGpuColor);
            mainWindow.progressbarVRAM.Foreground = new SolidColorBrush(defaultGpuColor);

            var gradientBrush = mainWindow.mainWindowBorder.BorderBrush as GradientBrush;
            if (gradientBrush != null && gradientBrush.GradientStops.Count > 0)
            {
                gradientBrush.GradientStops[0].Color = defaultGpuColor;
            }

            Properties.Settings.Default.GpuColor = System.Drawing.Color.FromArgb(defaultGpuColor.A, defaultGpuColor.R, defaultGpuColor.G, defaultGpuColor.B);
            Properties.Settings.Default.Save();
        }





    }
}
