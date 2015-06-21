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
using HtmlAgilityPack;
using Tweetinvi;
using Tweetinvi.Core.Interfaces.Credentials;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace example
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            //we will do things when the page is loaded like getting the pincode
            twitterWebView.LoadCompleted += TwitterWebView_LoadCompleted;
        }

        private async void TwitterWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string html = await twitterWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            //install html agility pack
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var pin = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.Id == "oauth_pin");
            if (pin != null)
            {
                //we got the pin and need to generate our tokens new tokens
                if (_temporaryCredentials != null)
                {
                    var credentials = CredentialsCreator.GetCredentialsFromVerifierCode(pin.InnerHtml,
                        _temporaryCredentials);
                    //set them so we can tweet
                    TwitterCredentials.SetCredentials(credentials.AccessToken, credentials.AccessTokenSecret,
                    credentials.ConsumerKey, credentials.ConsumerSecret);
                }
                //remove the popup
                popupStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private ITemporaryCredentials _temporaryCredentials;
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            //show the login "popup"
            popupStackPanel.Visibility = Visibility.Visible;
            //generate the url we need to display this is from the tweetvini library
            _temporaryCredentials =
                CredentialsCreator.GenerateApplicationCredentials(consumerKey: "YOUR_CONSUMERKEY_HERE",
                    consumerSecret: "YOUR_SECRET_HERE");
            var url = CredentialsCreator.GetAuthorizationURL(_temporaryCredentials);

            //show the url on wich the user needs to login
            twitterWebView.Source = new Uri(url);
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tweetTextBox.Text))
            {
                Tweet.PublishTweet(tweetTextBox.Text);
                tweetTextBox.Text = "";
            }
        }

        private void PopupClose(object sender, RoutedEventArgs e)
        {
            popupStackPanel.Visibility = Visibility.Collapsed;
        }
    }
}
