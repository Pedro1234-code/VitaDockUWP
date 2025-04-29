using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.Gaming.Input;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Media.MediaProperties;
using Windows.Storage;


// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace VitaDock_CameraUWP
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private bool isPreviewing;

        public MainPage()
        {
            this.InitializeComponent();
            Gamepad.GamepadAdded += Gamepad_GamepadAdded;
        }

        private Gamepad currentGamepad;
        private DateTime startButtonPressTime;

        private void Gamepad_GamepadAdded(object sender, Gamepad e)
        {
            currentGamepad = e;
            StartListeningForInput();
        }

        private async void StartListeningForInput()
        {
            while (true)
            {
                if (currentGamepad != null)
                {
                    var reading = currentGamepad.GetCurrentReading();

                    if ((reading.Buttons & GamepadButtons.Menu) != 0) // Start Button
                    {
                        if (startButtonPressTime == DateTime.MinValue)
                        {
                            startButtonPressTime = DateTime.Now;
                        }
                        else if ((DateTime.Now - startButtonPressTime).TotalSeconds >= 2) // Hold for 2 seconds
                        {
                            startButtonPressTime = DateTime.MinValue;
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                Frame.Navigate(typeof(SettingsPage));
                            });
                        }
                    }
                    else
                    {
                        startButtonPressTime = DateTime.MinValue;
                    }
                }

                await Task.Delay(100); // Check every 100ms
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await StartPreviewAsync();
        }

        private async Task StartPreviewAsync()
        {
            mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
            };

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count > 0)
            {
                settings.VideoDeviceId = devices[0].Id;
            }

            await mediaCapture.InitializeAsync(settings);

            // Load selected resolution from settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string selectedResolution = localSettings.Values["SelectedResolution"] as string ?? "1280x720";

            int width = 1280, height = 720;
            switch (selectedResolution)
            {
                case "960x544": width = 960; height = 544; break;
                case "896x504": width = 896; height = 504; break;
                case "864x488": width = 864; height = 488; break;
                case "480x272": width = 480; height = 272; break;
            }

            // Apply resolution
            var availableProperties = mediaCapture.VideoDeviceController
                .GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord)
                .OfType<VideoEncodingProperties>();

            var desiredProperties = availableProperties
                .Where(p => p.Width == width && p.Height == height)
                .FirstOrDefault();

            if (desiredProperties != null)
            {
                await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, desiredProperties);
            }

            captureElement.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();
            isPreviewing = true;
        }
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            await StopPreviewAsync();
        }

        private async Task StopPreviewAsync()
        {
            if (isPreviewing)
            {
                await mediaCapture.StopPreviewAsync();
                isPreviewing = false;
            }
        }
    }
}