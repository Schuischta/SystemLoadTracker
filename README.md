# SystemLoadTracker

## Description
The System Load Tracker is a lightweight WPF application that monitors key system performance indicators in real-time. It includes CPU, GPU, VRAM, and RAM usage, as well as temperature readings. Designed for ease of use, it lets users monitor their computer's performance easily.

## Features
- **CPU Monitoring:** Shows CPU usage and temperature, along with average speeds of CPU cores.
- **GPU Monitoring:** Shows GPU usage, how much VRAM is being used, GPU temperature, and its clock speed.
- **RAM Monitoring:** Shows how much RAM is being used.
- **Real-Time Updates:** Refreshes the data every second to give the latest info.
- **Settings:** Have a look at the screenshot below.

## Screenshot
![SLT-Main-Dark](https://github.com/Schuischta/SystemLoadTracker/assets/35001838/67b5b9dd-615e-4207-b73b-437943f2dee2)
![SLT-Main-Light](https://github.com/Schuischta/SystemLoadTracker/assets/35001838/86ceba45-ae17-4d56-bfcd-0d36a76dc284)

![SLT-Settings-Dark](https://github.com/Schuischta/SystemLoadTracker/assets/35001838/b27c670d-79f6-4942-9ed9-2db050404718)
![SLT-Settings-Light](https://github.com/Schuischta/SystemLoadTracker/assets/35001838/9107ed17-b7ef-46f3-92ee-d2755e426e3d)



## Requirements
- .NET Runtime 8.0
- Windows 10/11
- [Segoe Fluent Icons Font](https://learn.microsoft.com/en-us/windows/apps/design/downloads/#fonts) (pre-installed on Windows 11)

## Installation

### From Releases
1. Download the latest version of the app as a ZIP file from the [Releases Tab](https://github.com/Schuischta/SystemLoadTracker/releases) on this repository.
2. Unzip the file.
3. Run the `SystemLoadTracker.exe` file to open the application.

### Using winget
1. Open `Terminal` or `PowerShell`.
2. Execute this command:
```
winget install Schuischta.SystemLoadTracker
```
4. Go to `C:\Users\YOUR_USER\AppData\Local\Microsoft\WinGet\Packages\Schuischta.SystemLoadTracker_Microsoft.Winget...`
5. Run the `SystemLoadTracker.exe` file to open the application.

## Contributing
We welcome contributions! If you have suggestions or find issues, please open an issue or submit a pull request.

## License
This project is under the MIT License - see the [LICENSE](https://github.com/Schuischta/SystemLoadTracker/blob/master/LICENSE) file for details.
