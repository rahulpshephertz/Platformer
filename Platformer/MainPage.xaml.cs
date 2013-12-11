using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Facebook;
using System.IO;
using System.Windows.Media.Imaging;
using Platformer.App42;
using com.shephertz.app42.paas.sdk.windows;
using System.IO.IsolatedStorage;
using com.shephertz.app42.paas.sdk.windows.social;
using com.shephertz.app42.paas.sdk.windows.message;

namespace Platformer
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const String ExtendedPermissions = "read_stream,publish_stream,offline_access,publish_actions,user_location, user_birthday";
        private readonly FacebookClient _fb = new FacebookClient();
        private WebBrowser _webBrowser;
        private string _Uri = "";
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            MessagePopup.Visibility = Visibility.Collapsed;
            base.OnNavigatedFrom(e);
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (brdFacebook.Visibility == Visibility.Visible)
            {
                brdFacebook.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (Convert.ToInt32(btn.Tag.ToString()))
            {
                case 0:
                    _Uri = "/GamePage.xaml";
                    break;
                case 1:
                    _Uri = "/ScoreBoard.xaml";
                    break;
            }
            if (!DBManager.getInstance().isDBAvailable(DBManager.DB_Profile))
            {
                progressBarText.Visibility = Visibility.Visible;
                progressBarText.Text = "Connecting you to Facebook";
                progressBar.IsIndeterminate = true;
                progressBar.Visibility = Visibility.Visible;
                LoginWithFB();
            }
            else
            {
                NavigationService.Navigate(new Uri(_Uri, UriKind.Relative));
            }
        }
        private void LoginWithFB()
        {
            var loginUrl = GetFacebookLoginUrl(GlobalContext.facebookAppId, ExtendedPermissions);
            //Add webBrowser to the contentPanel
            _webBrowser = new WebBrowser();
            _webBrowser.Navigate(loginUrl);
            //_webBrowser.Visibility = Visibility.Visible;
            Image imgClose = new Image();
            imgClose.Tap += imgClose_Tap;
            imgClose.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            imgClose.VerticalAlignment = VerticalAlignment.Top;
            imgClose.Width = 56;
            imgClose.Height = 56;
            imgClose.Margin = new Thickness(-20, -25, 0, 0);
            imgClose.Source = new BitmapImage(new Uri("Images/closeIcon.png", UriKind.Relative));
            grdFacebook.Children.Add(_webBrowser);
            grdFacebook.Children.Add(imgClose);
            ProgressBarPanel.Visibility = Visibility.Visible;
            _webBrowser.Visibility = Visibility.Visible;
            brdFacebook.Visibility = Visibility.Visible;
            _webBrowser.Navigated += WebBrowser_Navigated;
        }

        private Uri GetFacebookLoginUrl(String appId, String extendedPermissions)
        {
            var parameters = new Dictionary<String, object>();
            parameters["client_id"] = appId;
            parameters["redirect_uri"] = "https://m.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            // add the 'scope' only if we have extendedPermissions.
            if (!String.IsNullOrEmpty(extendedPermissions))
            {
                // A comma-delimited list of permissions
                parameters["scope"] = extendedPermissions;
            }
            return _fb.GetLoginUrl(parameters);
        }
        void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            ProgressBarPanel.Visibility = Visibility.Collapsed;
            if (!_fb.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
            {
                return;
            }
            if (oauthResult.IsSuccess)
            {
                //AccessToken is used when you want to use API as a user
                GlobalContext.AccessToken = oauthResult.AccessToken;
                if (IsolatedStorageSettings.ApplicationSettings.Contains("AccessToken"))
                {
                    IsolatedStorageSettings.ApplicationSettings["AccessToken"] = GlobalContext.AccessToken;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("AccessToken", GlobalContext.AccessToken);
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                MyProfile();
                MessagePopup.Visibility = Visibility.Visible;
                _webBrowser.Visibility = System.Windows.Visibility.Collapsed;
                grdFacebook.Children.Clear();
                brdFacebook.Visibility = Visibility.Collapsed;
            }
            else
            {
                // user cancelled
                MessageBox.Show(oauthResult.ErrorDescription);
            }
        }
        // My facebook feed.
        internal void MyProfile()
        {
            GlobalContext.g_UserProfile = new UserProfile();
            var fb = new FacebookClient(GlobalContext.AccessToken);
            fb.GetCompleted +=
                (o, ex) =>
                {
                    try
                    {
                        var feed = (IDictionary<String, object>)ex.GetResultData();
                        GlobalContext.g_UserProfile.Name = feed["name"].ToString();
                        GlobalContext.g_UserProfile.UserID = feed["id"].ToString();
                        GlobalContext.g_UserProfile.Picture = (String)((IDictionary<String, object>)((IDictionary<String, object>)feed["picture"])["data"])["url"];
                        // GlobalContext.g_UserProfile.Picture = picture.data.url;
                        DBManager.getInstance().saveData(DBManager.DB_Profile, GlobalContext.g_UserProfile);
                        Deployment.Current.Dispatcher.BeginInvoke(delegate(){
                            App42Api.LinkUserFacebookAccount(LinkUserFacebookCallback);
                        });
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                };
            var parameters = new Dictionary<String, object>();
            parameters["fields"] = "id,name,picture";
            fb.GetAsync("me", parameters);
        }
        private void LinkUserFacebookCallback(object response,bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
            }
            else
            {
                Social social = (Social)response;
                if (social.IsResponseSuccess())
                {
                    GlobalContext.isFacebookAccountLinkedToApp42 = true;
                }
                else
                {
                    GlobalContext.isFacebookAccountLinkedToApp42 = false; 
                }
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("IsLinkedToFacebook"))
            {
                IsolatedStorageSettings.ApplicationSettings["IsLinkedToFacebook"] = GlobalContext.isFacebookAccountLinkedToApp42;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add("IsLinkedToFacebook", GlobalContext.isFacebookAccountLinkedToApp42);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            NavigationService.Navigate(new Uri(_Uri, UriKind.Relative));
        }
        void imgClose_Tap(object sender, RoutedEventArgs e)
        {
            brdFacebook.Visibility = Visibility.Collapsed;
        }
        private void NavigateToNextPage()
        {
            NavigationService.Navigate(new Uri(_Uri, UriKind.Relative));
        }
    }
}