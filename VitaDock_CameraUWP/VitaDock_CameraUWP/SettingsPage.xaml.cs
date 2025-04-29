using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;

namespace VitaDock_CameraUWP
{
    public sealed partial class SettingsPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsPage()
        {
            this.InitializeComponent();
            LoadSavedResolution();
        }

        private void LoadSavedResolution()
        {
            if (localSettings.Values.ContainsKey("SelectedResolution"))
            {
                string savedResolution = localSettings.Values["SelectedResolution"].ToString();
                foreach (ComboBoxItem item in ResolutionComboBox.Items)
                {
                    if (item.Content.ToString() == savedResolution)
                    {
                        ResolutionComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResolutionComboBox.SelectedItem != null)
            {
                localSettings.Values["SelectedResolution"] = (ResolutionComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
