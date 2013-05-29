using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace All_Docs
{
    public sealed partial class SettingsFlyout : All_Docs.Common.LayoutAwarePage
    {
        // The guidelines recommend using 100px offset for the content animation.
        const int ContentAnimationOffset=0;

        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        MainPage rootPage = MainPage.Current;

        ApplicationDataContainer roamingSettings = null;

        public SettingsFlyout()
        {
           
            this.InitializeComponent();
            roamingSettings = ApplicationData.Current.RoamingSettings;
            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });
            ListAccounts();
        }

        private void ListAccounts()
        {

            if (roamingSettings.Containers.ContainsKey("Accounts"))
            {
                var values = roamingSettings.Containers["Accounts"].Values.ToList();
                foreach (var keyValuePair in values)
                {
                    StackPanel panel = new StackPanel();
                    panel.Margin = new Thickness(0,20,0,0);
                    panel.Orientation=Orientation.Horizontal;
                   
                    ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)keyValuePair.Value;
                    
                    TextBlock type = new TextBlock();
                    string typeInt = "" + composite["Type"];
                    type.Text = typeInt == "0" ? "SkyDrive" : typeInt =="1" ? "Google Drive" : typeInt=="2" ? "Dropbox" : "Box";
                    type.Margin = new Thickness(10, 10, 0, 0);
                    type.FontSize = 15;
                    type.Width = 100;
                    
                    TextBlock login = new TextBlock();
                    login.Text = ""+composite["Login"];
                    login.Margin = new Thickness(10, 10, 0, 0);
                    login.FontSize = 15;
                    login.FontWeight = FontWeights.Bold;
                    login.Width = 300;

                    Button delete = new Button();
                    delete.Tag = ""+composite["Type"] + composite["Login"];
                    delete.Margin = new Thickness(10, 0, 0, 0);
                    delete.Content = "Delete";
                    delete.Click += DeleteAccountButton_Click;

                    panel.Children.Add(type);
                    panel.Children.Add(login);
                    panel.Children.Add(delete);
                    panel.VerticalAlignment = VerticalAlignment.Center;

                    AccountItems.Children.Add(panel);
                }
            }

        }

        /// <summary>
        /// This is the click handler for the back button on the Flyout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MySettingsBackClicked(object sender, RoutedEventArgs e)
        {
            roamingSettings = ApplicationData.Current.RoamingSettings;
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }

            if (!roamingSettings.Containers.ContainsKey("Accounts") || roamingSettings.Containers["Accounts"].Values.Count == 0)
            {
                rootPage.AddNewAccount();

            }

            // If the app is not snapped, then the back button shows the Settings pane again.
            if (Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                SettingsPane.Show();
            }
        }

        /// <summary>
        /// This is the a common click handler for the buttons on the Flyout.  You would replace this with your own handler
        /// if you have a button or buttons on this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                // First close our Flyout.
                Popup parent = this.Parent as Popup;
                if (parent != null)
                {
                    parent.IsOpen = false;
                }
                rootPage.AddNewAccount();
            }
        }

        async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {

                var messageDialog = new MessageDialog("Are you sure you want to delete this account: " + (""+b.Tag).Substring(1) +"?", "Are you sure?");
                bool? result = null;
                messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler((cmd) => result = true)));
                messageDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler((cmd) => result = false)));
                
                await messageDialog.ShowAsync();

                if (result == true)
                {
                    HttpClient client = new HttpClient();

                    string setting = "" + b.Tag;
                    roamingSettings = ApplicationData.Current.RoamingSettings;
                    //
                    ApplicationDataCompositeValue composite = roamingSettings.Containers["Accounts"].Values[setting] as ApplicationDataCompositeValue;
                    string token = ""+ composite["AccessToken"];

                    string url = "";
                    if ("" + composite["Type"] == "1")//revoke google
                    {

                        HttpResponseMessage res1 =
                            await client.GetAsync("https://accounts.google.com/o/oauth2/revoke?token=" + token);

                        if (res1.IsSuccessStatusCode)
                        {
                        }
                    }


                    roamingSettings.Containers["Accounts"].Values.Remove(setting);

                        foreach (StackPanel child in AccountItems.Children)
                        {
                            foreach (var pan in child.Children)
                            {
                                if (pan is Button && ((Button) pan).Tag == b.Tag)
                                    AccountItems.Children.Remove(child);
                                break;
                            }

                        }
                    
                }
                roamingSettings = ApplicationData.Current.RoamingSettings;
                if (!roamingSettings.Containers.ContainsKey("Accounts") ||
                    roamingSettings.Containers["Accounts"].Values.Count == 0)
                {
                    Popup parent = this.Parent as Popup;
                    // First close our Flyout.
                    if (parent != null)
                    {
                        parent.IsOpen = false;
                    }
                    rootPage.AddNewAccount();

                }
                else
                {
                    SettingsPane.Show();
                }
            }
        }
    }
}
