using LibreHardwareMonitor.Hardware;
using SharpDX.Direct3D11;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Management;

namespace SystemLoadTracker
{
    public partial class MainWindow : Window
    {

        private const float VramConversionFactor = 1024f;

        private readonly Computer computer;
        private DispatcherTimer? timer;

        private int lastGpuLoadValue = -1;
        private int lastCpuLoadValue = -1;
        private int lastVramLoadValue = -1;
        private int lastRamLoadValue = -1;

        private readonly long totalVram;
        private readonly long totalRam;

        public MainWindow()
        {
            InitializeComponent();
            computer = new Computer();
            totalVram = GetTotalVRAM();
            totalRam = GetTotalRAM();
            InitializeSettings();
            InitializeTimer();
            InitializeComputer();
        }


        private void InitializeSettings()
        {
            // Set the window position
            this.Closing += MainWindow_Closing;

            if (!double.IsNaN(Properties.Settings.Default.WindowTop))
            {
                this.Top = Properties.Settings.Default.WindowTop;
                this.Left = Properties.Settings.Default.WindowLeft;
            }

            // Set the FixedWindow property based on the saved settings
            canMoveWindow = Properties.Settings.Default.FixedWindow;

            // Set Topmost based on the saved setting
            this.Topmost = Properties.Settings.Default.AlwaysOnTop;

            // Set ShowBorder based on the saved setting
            ToggleMainWindowBorder(Properties.Settings.Default.ShowMainWindowBorder);

            SetCornerRadius(Properties.Settings.Default.MainWindowCornerRadius);
        }

        private void InitializeComputer()
        {
            computer.IsCpuEnabled = true;  // Enable CPU monitoring
            computer.IsGpuEnabled = true;  // Enable GPU monitoring
            computer.IsMemoryEnabled = true;  // Enable RAM monitoring

            LoadWindowSettings();

            try
            {
                computer.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing hardware monitoring: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.RefreshInterval);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void UpdateTimerInterval(double interval)
        {
            if (timer != null)
            {
                timer.Interval = TimeSpan.FromSeconds(interval);
            }
        }



        // Timer tick event to refresh sensor values
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var hardwareItem in computer.Hardware)
                    {
                        hardwareItem.Update();
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show($"Error updating hardware data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }

                Dispatcher.BeginInvoke(() =>
                {
                    UpdateUIElements();
                    UpdateClocks();
                });
            });
        }

        // Updates UI elements based on the latest hardware data
        private void UpdateUIElements()
        {
            foreach (var hardwareItem in computer.Hardware)
            {
                UpdateHardwareUI(hardwareItem);
            }
        }

        // Updates UI based on the type of hardware
        private void UpdateHardwareUI(IHardware hardwareItem)
        {
            switch (hardwareItem.HardwareType)
            {
                case HardwareType.Cpu:
                    UpdateCpuUI(hardwareItem);
                    break;
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                    UpdateGpuUI(hardwareItem);
                    break;
                case HardwareType.Memory:
                    UpdateMemoryUI(hardwareItem);
                    break;
            }
        }

        // Updates UI elements related to CPU
        private void UpdateCpuUI(IHardware cpu)
        {
            var loadSensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
            var tempSensorNames = new[] { "CPU Package", "Core (Tctl/Tdie)" }; // List of sensor names to be searched
            var tempSensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && tempSensorNames.Contains(s.Name));

            UpdateProgressBar(labelCPUload, progressbarCPU, GetSensorValue(loadSensor), ref lastCpuLoadValue, "");
            if (tempSensor != null)
            {
                UpdateTemperature(labelCPUtemp, progressbarCPUtemp, tempSensor, "");
            }

            // Search for power sensors
            var powerSensorNames = new[] { "CPU Package", "Package" }; // List of sensor names to be searched
            var powerSensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power && powerSensorNames.Contains(s.Name));
            if (powerSensor != null)
            {
                labelCPUwatts.Content = $"{GetSensorValue(powerSensor):N1} W"; // Update CPU power label in the UI
            }
        }


        // Updates UI elements related to GPU
        private long GetTotalVRAM()
        {
            using var device = new Device(SharpDX.Direct3D.DriverType.Hardware);
            using var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device2>();
            using var adapter = dxgiDevice.Adapter;
            using var dxgiAdapter = adapter.QueryInterface<SharpDX.DXGI.Adapter3>();
            return dxgiAdapter.Description.DedicatedVideoMemory;
        }

        private void UpdateGpuUI(IHardware gpu)
        {
            float gpuCoreLoad = 0;
            float vramUsed = 0;

            foreach (var sensor in gpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                {
                    gpuCoreLoad = GetSensorValue(sensor);
                    UpdateProgressBar(labelGPUload, progressbarGPU, gpuCoreLoad, ref lastGpuLoadValue, "");
                }
                // Check for "GPU Memory Used" for NVIDIA or "D3D Dedicated Memory Used" for AMD
                else if (sensor.SensorType == SensorType.SmallData && (sensor.Name == "GPU Memory Used" || sensor.Name == "D3D Dedicated Memory Used"))
                {
                    vramUsed = GetSensorValue(sensor);
                }
                else if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Core"))
                {
                    UpdateTemperature(labelGPUtemp, progressbarGPUtemp, sensor, "");
                }
            }

            if (totalVram > 0)
            {
                // Calculate VRAM usage based on the total capacity of the VRAM and the amount already used
                float vramUsage = (vramUsed / (totalVram / VramConversionFactor / VramConversionFactor)) * 100; // Convert totalVram to MB
                UpdateProgressBar(labelVRAM, progressbarVRAM, vramUsage, ref lastVramLoadValue, "", totalVram);
            }

            // Search for power sensors
            var powerSensor = gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power && s.Name == "GPU Package");
            if (powerSensor != null)
            {
                labelGPUwatts.Content = $"{GetSensorValue(powerSensor):N1} W"; // Update GPU power label in the UI
            }
        }

        // Updates UI elements related to RAM

        private long GetTotalRAM()
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            using var collection = searcher.Get();
            foreach (var item in collection)
            {
                return Convert.ToInt64(item["TotalPhysicalMemory"]);
            }
            return 0;
        }


        private void UpdateMemoryUI(IHardware ram)
        {
            var loadSensor = ram.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
            UpdateProgressBar(labelRAM, progressbarRAM, GetSensorValue(loadSensor), ref lastRamLoadValue, "", totalRam);
        }


        // Retrieves the current value from a sensor, returns 0 if the sensor is null
        private float GetSensorValue(ISensor? sensor)
        {
            return sensor?.Value ?? 0;
        }


        // Updates the progress bar and label for load and temperature sensors
        private void UpdateProgressBar(Label label, ProgressBar progressBar, float value, ref int lastValue, string labelPrefix, long totalMemory = 0)
        {
            if ((int)value != lastValue)
            {
                progressBar.Value = (int)value;
                if (totalMemory > 0)
                {
                    // Convert the value from percentage to GB
                    float usedMemory = (value / 100) * totalMemory / (1024 * 1024 * 1024);
                    label.Content = $"{labelPrefix}{usedMemory:N1} GB";
                }
                else
                {
                    label.Content = $"{labelPrefix}{value:N0}";
                }
                lastValue = (int)value;
            }
        }

        private void UpdateTemperature(Label label, ProgressBar progressBar, ISensor sensor, string labelPrefix)
        {
            float value = GetSensorValue(sensor);
            progressBar.Value = (int)value;
            label.Content = $"{labelPrefix}{value:N0}°C";
        }


        // Method to update clock speed for CPU and GPU
        private void UpdateClocks()
        {
            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    float totalCpuClock = 0;
                    int cpuCount = 0;

                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core"))
                        {
                            float clockValue = sensor.Value.GetValueOrDefault();
                            if (clockValue > 0)  // Consider valid values
                            {
                                totalCpuClock += clockValue;
                                cpuCount++;
                            }
                        }
                    }

                    if (cpuCount > 0)
                    {
                        float averageCpuClock = totalCpuClock / cpuCount;  // Calculate the average
                        Dispatcher.BeginInvoke(() =>
                        {
                            labelCpuClock.Content = $"{averageCpuClock:N0}";  // Update CPU clock speed label in the UI
                        });
                    }
                }
                else if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                {
                    var clockSensor = hardwareItem.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock && s.Name.Equals("GPU Core"));
                    if (clockSensor != null)
                    {
                        float gpuClock = clockSensor.Value.GetValueOrDefault();
                        Dispatcher.BeginInvoke(() =>
                        {
                            labelGpuClock.Content = $"{gpuClock:N0}";  // Update GPU clock speed label in the UI
                        });
                    }
                }
            }
        }



        // Ensures proper resource release on window close
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            computer.Close();
        }



        // UI event handlers for interaction
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

        // Allows the window to be moved by dragging
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && canMoveWindow)
            {
                DragMove();
            }
        }



        // Open the settings window
        private void settingsButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings settingsWindow = new Settings(this.Opacity, Properties.Settings.Default.MainWindowCornerRadius, this);

            settingsWindow.ShowDialog();
        }

        private void settingsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            settingsButton.Background = Brushes.Gray;
        }

        private void settingsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            settingsButton.Background = Brushes.Transparent;
        }



        // makes closeButton and settingsButton invisible when mouse is not over the window
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Visibility = Visibility.Hidden;
            settingsButton.Visibility = Visibility.Hidden;
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Visibility = Visibility.Visible;
            settingsButton.Visibility = Visibility.Visible;
        }




        // Settings

        // Load saved settings
        private void LoadWindowSettings()
        {
            this.Top = Properties.Settings.Default.WindowTop;
            this.Left = Properties.Settings.Default.WindowLeft;
            SetOpacity(Properties.Settings.Default.MainWindowOpacity);
        }

        // Sets the transparency
        public void SetOpacity(double opacity)
        {
            this.Opacity = opacity;
        }

        // Saves window position
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.WindowTop = this.Top;
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.Save();
        }

        // Sets the "Always on top" property
        public void ToggleAlwaysOnTop()
        {
            bool isCurrentlyOnTop = Properties.Settings.Default.AlwaysOnTop;
            this.Topmost = !isCurrentlyOnTop;
            Properties.Settings.Default.AlwaysOnTop = !isCurrentlyOnTop;
            Properties.Settings.Default.Save();
        }

        // Enables or disables window dragging
        public bool canMoveWindow = true;

        public void EnableWindowMove()
        {
            canMoveWindow = true;
        }

        public void DisableWindowMove()
        {
            canMoveWindow = false;
        }

        // Toggles the visibility of the main window border
        public void ToggleMainWindowBorder(bool showBorder)
        {
            mainWindowBorder.Visibility = showBorder ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.ShowMainWindowBorder = showBorder;
            Properties.Settings.Default.Save();
        }

        public void SetCornerRadius(double cornerRadius)
        {
            // Setzen Sie den Corner Radius für die gewünschten Elemente
            mainWindowCorner.CornerRadius = new CornerRadius(cornerRadius);
            mainWindowBorder.CornerRadius = new CornerRadius(cornerRadius);

            closeButtonBorder.CornerRadius = new CornerRadius(0, cornerRadius, 0, 0);
        }





    }
}