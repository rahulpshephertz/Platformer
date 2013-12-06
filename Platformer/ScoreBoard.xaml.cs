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
using Platformer.App42;
using System.Collections.ObjectModel;
using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.social;
using System.Windows.Media.Imaging;
using com.shephertz.app42.paas.sdk.windows.message;
using System.Windows.Threading;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.shephertz.app42.paas.sdk.windows.storage;

namespace Platformer
{
    public partial class ScoreBoard : PhoneApplicationPage
    {
        ObservableCollection<ScoreListItem> _scorelist = new ObservableCollection<ScoreListItem>();
        ObservableCollection<ScoreListItem> _friendscorelist = new ObservableCollection<ScoreListItem>();
        ObservableCollection<FacebookFriendItem> _friendlist = new ObservableCollection<FacebookFriendItem>();
        ObservableCollection<MessageItem> _messagelist = new ObservableCollection<MessageItem>();
        bool IsLoaded_scorelist = false, IsLoaded_friendscorelist = false;
        public ScoreBoard()
        {
            InitializeComponent();
            setMyProfile();
            App42Log.SetDebug(true);
            lbxFriendsScore.ItemsSource = _friendscorelist;
            lbxGlobalScore.ItemsSource = _scorelist;
            // lbxFriendsMyProfile.ItemsSource = _friendlist;
            lbxMessageMyProfile.ItemsSource = _messagelist;
        }
        private void setMyProfile()
        {
            try
            {
                imgProfile.Source = new BitmapImage(new Uri(GlobalContext.g_UserProfile.Picture, UriKind.Absolute));
                tblProfileName.Text = GlobalContext.g_UserProfile.Name;
                tblProfileScore.Text = "Score :" + GlobalContext.g_UserProfile.Score.ToString();
                tblProfileRank.Text = "Rank :" + GlobalContext.g_UserProfile.Rank.ToString();
            }
            catch (Exception e)
            {

            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //App42Api.GetFacebookFriendsFromLinkUser(GetUserFacebookFriendCallback);
            App42Api.GetTopNFacebookFriendsScores(10, GetTopNRankersFacebookFriendsCallback);
            App42Api.GetTopNGlobalScores(10, GetTopNRankersCallback);
            App42Api.GetUserRanking(GetUserRankCallback);
            App42Api.GetUserHighestScore(GetUserScoreCallback);
            App42Api.GetMessages(GetMessagesCallback);
            base.OnNavigatedTo(e);
        }
        private void subHeadersHighScores_btnTap(object sender, GestureEventArgs e)
        {
            int btnSelected = Convert.ToInt32((sender as Border).Tag.ToString());
            switch (btnSelected)
            {
                case 0: btnFriendsHighScore.Background = new SolidColorBrush(Colors.DarkGray);
                    btnGlobalHighScore.Background = new SolidColorBrush(Colors.Brown);
                    tblSendMessageChallenge.Visibility = Visibility.Visible;
                    if (IsLoaded_friendscorelist)
                    {
                        lbxFriendsScore.Visibility = Visibility.Visible;
                        lbxFriendsScoreMessagePopup.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        lbxFriendsScore.Visibility = Visibility.Collapsed;
                        lbxFriendsScoreMessagePopup.Visibility = Visibility.Visible;
                    }
                    lbxGlobalScoreMessagePopup.Visibility = Visibility.Collapsed;
                    lbxGlobalScore.Visibility = Visibility.Collapsed;
                    lbxFriendsScore.ItemsSource = _friendscorelist;
                    break;
                case 1: btnFriendsHighScore.Background = new SolidColorBrush(Colors.Brown);
                    btnGlobalHighScore.Background = new SolidColorBrush(Colors.DarkGray);
                    tblSendMessageChallenge.Visibility = Visibility.Collapsed;
                    if (IsLoaded_scorelist)
                    {
                        lbxGlobalScore.Visibility = Visibility.Visible;
                        lbxGlobalScoreMessagePopup.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        lbxGlobalScore.Visibility = Visibility.Collapsed;
                        lbxGlobalScoreMessagePopup.Visibility = Visibility.Visible;
                    }
                    lbxGlobalScore.ItemsSource = _scorelist;
                    lbxFriendsScoreMessagePopup.Visibility = Visibility.Collapsed;
                    lbxFriendsScore.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void btnSendMessage_Tap(object sender, GestureEventArgs e)
        {
            ScoreListItem item = (ScoreListItem)((sender as Border).Tag);
            tblToMessage.Text = "To : " + item.UserName;
            tblToMessage.Tag = item.UserId;
            messagePopup.Visibility = Visibility.Visible;
        }

        private void GetTopNRankersFacebookFriendsCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                //showMessage("Exception,Please try again later");
            }
            else
            {
                com.shephertz.app42.paas.sdk.windows.game.Game game = (com.shephertz.app42.paas.sdk.windows.game.Game)response;
                if (game.IsResponseSuccess())
                {

                    for (int i = 0; i < game.GetScoreList().Count; i++)
                    {
                        ScoreListItem item = new ScoreListItem();
                        item.Rank = (i + 1).ToString();
                        item.Score = game.GetScoreList()[i].GetValue().ToString();
                        item.UserId = game.GetScoreList()[i].GetFacebookProfile().GetId();
                        item.UserName = game.GetScoreList()[i].GetFacebookProfile().GetName();
                        _friendscorelist.Add(item);
                    }
                    lbxFriendsScore.ItemsSource = _friendscorelist;
                    if (_friendscorelist.Count == 0)
                    {
                        lbxFriendsScoremessageTB.Text = "There is no item in list";
                    }
                    else
                    {
                        IsLoaded_friendscorelist = true;
                        lbxFriendsScoreMessagePopup.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    lbxFriendsScoremessageTB.Text = "Error,Please try again later";
                    // showMessage("Error,Please try again later");
                }
            }
        }
        private void GetTopNRankersCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                //showMessage("Exception,Please try again later");
            }
            else
            {
                List<string> userIds = new List<string>();
                com.shephertz.app42.paas.sdk.windows.game.Game game = (com.shephertz.app42.paas.sdk.windows.game.Game)response;
                if (game.IsResponseSuccess())
                {
                    for (int i = 0; i < game.GetScoreList().Count; i++)
                    {
                        ScoreListItem item = new ScoreListItem();
                        item.Rank = (i + 1).ToString();
                        item.Score = game.GetScoreList()[i].GetValue().ToString();
                        item.UserName = game.GetScoreList()[i].GetUserName();
                        _scorelist.Add(item);
                        userIds.Add(game.GetScoreList()[i].GetUserName());
                    }
                    App42Api.GetFacebookProfilesFromIds(userIds, GetUserFacebookProfileFromIdsCallback);
                }
                else
                {
                    lbxGlobalScoremessageTB.Text = "Error,Please try again later";
                }
            }
        }
        private void GetUserScoreCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                //showMessage("Exception,Please try again later");
            }
            else
            {
                com.shephertz.app42.paas.sdk.windows.game.Game game = (com.shephertz.app42.paas.sdk.windows.game.Game)response;
                if (game.IsResponseSuccess())
                {
                    GlobalContext.g_UserProfile.Score = Convert.ToInt32(game.GetScoreList()[0].GetValue());
                    setMyProfile();
                }
                else
                {
                    // showMessage("Error,Please try again later");
                }
            }
        }
        private void GetUserRankCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                //showMessage("Exception,Please try again later");
            }
            else
            {
                com.shephertz.app42.paas.sdk.windows.game.Game game = (com.shephertz.app42.paas.sdk.windows.game.Game)response;
                if (game.IsResponseSuccess())
                {
                    try
                    {
                        GlobalContext.g_UserProfile.Rank = Convert.ToInt32(game.GetScoreList()[0].GetRank());
                        setMyProfile();
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                    // showMessage("Error,Please try again later");
                }
            }
        }
        private void GetUserFacebookProfileFromIdsCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                lbxGlobalScoremessageTB.Text = "Error,Please try again later";
            }
            else
            {
                Social _response = (Social)response;
                for (int i = 0; i < _response.GetPublicProfile().Count; i++)
                {
                    _scorelist[i].UserName = _response.GetPublicProfile()[i].name;
                    _scorelist[i].Icon = _response.GetPublicProfile()[i].picture;
                }
                lbxGlobalScore.ItemsSource = _scorelist;
                IsLoaded_scorelist = true;
                lbxGlobalScoreMessagePopup.Visibility = Visibility.Collapsed;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int tag = Convert.ToInt32((sender as Button).Tag.ToString());
            switch (tag)
            {
                case 0: messagePopup.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    StringBuilder sbJson1 = new StringBuilder();
                    StringWriter sw = new StringWriter(sbJson1);
                    JsonWriter itemObj = new JsonTextWriter(sw);
                    itemObj.WriteStartObject();
                    itemObj.WritePropertyName("SenderName");
                    itemObj.WriteValue(GlobalContext.g_UserProfile.Name);
                    itemObj.WritePropertyName("SenderID");
                    itemObj.WriteValue(GlobalContext.g_UserProfile.UserID);
                    itemObj.WritePropertyName("Picture");
                    itemObj.WriteValue(GlobalContext.g_UserProfile.Picture);
                    itemObj.WritePropertyName("Message");
                    itemObj.WriteValue(tbxMessage.Text);
                    itemObj.WritePropertyName("RecepientID");
                    itemObj.WriteValue(tblToMessage.Tag.ToString());
                    itemObj.WriteEndObject();
                    App42Api.SendMessage(sbJson1.ToString(), SendMessagesCallback);
                    sendMessagePopupTB.Text = "Please wait...";
                    sendMessagePopup.Visibility = Visibility.Visible;
                    break;
            }
        }
        public void SendMessagesCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                sendMessagePopupTB.Text = "Error,Please try again later";
            }
            else
            {
                Storage storage = (Storage)response;
                sendMessagePopupTB.Text = "Message sent successfully";
            }
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }
        public void GetMessagesCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                if (exception.GetAppErrorCode() == 2601)
                {
                    lbxMessageMyProfileMessageTB.Text = "No Messages";
                }
                else
                {
                    lbxMessageMyProfileMessageTB.Text = "Error,Please try again later";
                }
            }
            else
            {
                Storage storage = (Storage)response;
                _messagelist.Clear();
                for (int i = 0; i < storage.GetJsonDocList().Count; i++)
                {
                    MessageItem item = new MessageItem();
                    JObject messageObject = JObject.Parse(storage.GetJsonDocList()[i].GetJsonDoc());
                    item.Message = messageObject["Message"].ToString();
                    item.SenderName = messageObject["SenderName"].ToString();
                    item.Icon = messageObject["Picture"].ToString();
                    item.MessageId = storage.GetJsonDocList()[i].GetDocId();
                    _messagelist.Add(item);
                }
                lbxMessageMyProfile.ItemsSource = _messagelist;
                lbxMessageMyProfileMessagePopup.Visibility = Visibility.Collapsed;
            }
        }
        MessageItem item;
        private void btnDeleteMyProfile_Tap(object sender, GestureEventArgs e)
        {
            item = (MessageItem)((sender as FrameworkElement).Tag);
            App42Api.DeleteMessage(item.MessageId, DeleteMessageCallback);
            lbxMessageMyProfileMessageTB.Text = "Please wait...";
            lbxMessageMyProfileMessagePopup.Visibility = Visibility.Visible;
        }
        public void DeleteMessageCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                lbxMessageMyProfileMessageTB.Text = "Error,Please try again later";
                lbxMessageMyProfileMessagePopup.Visibility = Visibility.Visible;
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();
            }
            else
            {
                App42Response app42response = (App42Response)response;
                if (app42response.IsResponseSuccess())
                {
                    _messagelist.Remove(item);
                    lbxMessageMyProfileMessagePopup.Visibility = Visibility.Collapsed;
                    lbxMessageMyProfile.ItemsSource = _messagelist;
                }
                else
                {
                    lbxMessageMyProfileMessageTB.Text = "Error,Please try again later";
                    lbxMessageMyProfileMessagePopup.Visibility = Visibility.Visible;
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 1);
                    timer.Tick += new EventHandler(timer_Tick);
                    timer.Start();
                }
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            lbxMessageMyProfileMessagePopup.Visibility = Visibility.Collapsed;
            sendMessagePopup.Visibility = Visibility.Collapsed;
            (sender as DispatcherTimer).Stop();
        }
        //private void subHeadersMyProfile_btnTap(object sender, GestureEventArgs e)
        //{
        //    int btnSelected = Convert.ToInt32((sender as Border).Tag.ToString());
        //    switch (btnSelected)
        //    {
        //        case 0: btnFriendsMyProfile.Background = new SolidColorBrush(Colors.DarkGray);
        //             btnMessagesMyProfile.Background = new SolidColorBrush(Colors.Brown);
        //             if (IsLoaded_friendlist)
        //             {
        //                 lbxFriendsMyProfile.Visibility = Visibility.Visible;
        //                 lbxFriendsMyProfileMessagePopup.Visibility = Visibility.Collapsed;
        //             }
        //             else
        //             {
        //                 lbxFriendsMyProfile.Visibility = Visibility.Collapsed;
        //                 lbxFriendsMyProfileMessagePopup.Visibility = Visibility.Visible;
        //             }
        //             lbxMessageMyProfileMessagePopup.Visibility = Visibility.Collapsed;
        //             lbxMessageMyProfile.Visibility = Visibility.Collapsed;
        //             lbxFriendsMyProfile.ItemsSource = _friendlist;
        //            break;
        //        case 1:btnFriendsMyProfile.Background = new SolidColorBrush(Colors.Brown);
        //              btnMessagesMyProfile.Background = new SolidColorBrush(Colors.DarkGray);
        //              if (IsLoaded_messagelist)
        //              {
        //                  lbxMessageMyProfile.Visibility = Visibility.Visible;
        //                  lbxMessageMyProfileMessagePopup.Visibility = Visibility.Collapsed;
        //              }
        //              else
        //              {
        //                  lbxMessageMyProfile.Visibility = Visibility.Collapsed;
        //                  lbxMessageMyProfileMessagePopup.Visibility = Visibility.Visible;
        //              }
        //              lbxFriendsMyProfileMessagePopup.Visibility = Visibility.Collapsed;
        //             lbxFriendsMyProfile.Visibility = Visibility.Collapsed;
        //             lbxMessageMyProfile.ItemsSource = _messagelist; 
        //            break;
        //    }
        //}
        //private void GetUserFacebookFriendCallback(object response, bool IsException)
        //{
        //    if (IsException)
        //    {
        //        App42Exception exception = (App42Exception)response;
        //        // lbxFriendsMyProfilemessageTB.Text = "Error,Please try again later";
        //    }
        //    else
        //    {
        //        _friendlist.Clear();
        //        Social _response = (Social)response;
        //        for (int i = 0; i < _response.GetFriendList().Count; i++)
        //        {
        //            FacebookFriendItem item1 = new FacebookFriendItem();
        //            item1.UserId = _response.GetFriendList()[i].GetId();
        //            item1.UserName = _response.GetFriendList()[i].GetName();
        //            item1.Icon = _response.GetFriendList()[i].GetPicture();
        //            _friendlist.Add(item1);
        //        }
        //        //  lbxFriendsMyProfile.ItemsSource = _friendlist;
        //        IsLoaded_friendlist = true;
        //        //  lbxFriendsMyProfileMessagePopup.Visibility = Visibility.Collapsed;
        //    }
        //}
    }
}