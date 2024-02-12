using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LibreHardwareMonitor.Hardware;
using SharpDX.Direct3D11;

namespace SystemLoadTracker
{
    public partial class MainWindow : Window
    {
        private Computer computer;
        private DispatcherTimer timer;

        private int lastGpuLoadValue = -1;
        private int lastCpuLoadValue = -1;
        private int lastVramLoadValue = -1;
        private int lastRamLoadValue = -1;

        private long totalVram;

        public MainWindow()
        {
            computer = new Computer();
            timer = new DispatcherTimer();
            InitializeComponent();
            InitializeComputer();
            SetupTimer();

            totalVram = GetTotalVRAM();
        }

        // Initializes the computer object for hardware monitoring
        private void InitializeComputer()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,  // Enable CPU monitoring
                IsGpuEnabled = true,  // Enable GPU monitoring
                IsMemoryEnabled = true  // Enable RAM monitoring
            };

            try
            {
                computer.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing hardware monitoring: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sets up the dispatcher timer for periodic UI updates
        private void SetupTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer1_Tick;
            timer.Start();
        }

        // Timer tick event to refresh sensor values
        private void Timer1_Tick(object? sender, EventArgs e)
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
                    Dispatcher.Invoke(() => MessageBox.Show($"Error updating hardware data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }

                Dispatcher.Invoke(() =>
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
        }


        // Updates UI elements related to GPU

        private long GetTotalVRAM()
        {
            using (var device = new Device(SharpDX.Direct3D.DriverType.Hardware))
            {
                using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device2>())
                {
                    using (var adapter = dxgiDevice.Adapter)
                    {
                        using (var dxgiAdapter = adapter.QueryInterface<SharpDX.DXGI.Adapter3>())
                        {
                            return dxgiAdapter.Description.DedicatedVideoMemory;
                        }
                    }
                }
            }
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
                float vramUsage = (vramUsed / (totalVram / 1024f / 1024f)) * 100; // Konvertiere vramTotal in MB
                UpdateProgressBar(labelVRAM, progressbarVRAM, vramUsage, ref lastVramLoadValue, "");
            }
        }






        // Updates UI elements related to RAM
        private void UpdateMemoryUI(IHardware ram)
        {
            var loadSensor = ram.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);
            UpdateProgressBar(labelRAM, progressbarRAM, GetSensorValue(loadSensor), ref lastRamLoadValue, "");
        }

        // Retrieves the current value from a sensor, returns 0 if the sensor is null
        private float GetSensorValue(ISensor? sensor)
        {
            return sensor?.Value ?? 0;
        }

        // Updates the progress bar and label for load and temperature sensors
        private void UpdateProgressBar(Label label, ProgressBar progressBar, float value, ref int lastValue, string labelPrefix)
        {
            if ((int)value != lastValue)
            {
                progressBar.Value = (int)value;
                label.Content = $"{labelPrefix}{value:N0}%";
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
                        Dispatcher.Invoke(() =>
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
                        Dispatcher.Invoke(() =>
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
            if (computer != null)
            {
                computer.Close();
            }
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