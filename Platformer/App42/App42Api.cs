using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.game;
using com.shephertz.app42.paas.sdk.windows.message;
using com.shephertz.app42.paas.sdk.windows.social;
using com.shephertz.app42.paas.sdk.windows.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Platformer.App42
{
    public class App42Api
    {
        private static SocialService mSocialService = null;
        private static ScoreBoardService mScoreBoardService = null;
        private static StorageService mStorageService = null;
        public delegate void App42ApiResultCallback(object response, bool isException);

        public static void LinkUserFacebookAccount(App42ApiResultCallback callBack)
        {
            App42ApiCallback _sCallback = new App42ApiCallback(callBack);
            if (mSocialService == null)
            {
                mSocialService = GlobalContext.SERVICE_API.BuildSocialService();
            }
            mSocialService.LinkUserFacebookAccount(GlobalContext.g_UserProfile.UserID, GlobalContext.AccessToken, _sCallback);
        }
        public static void GetFacebookProfile(App42ApiResultCallback callBack)
        {
            App42ApiCallback _sCallback = new App42ApiCallback(callBack);
            if (mSocialService == null)
            {
                mSocialService = GlobalContext.SERVICE_API.BuildSocialService();
            }
            mSocialService.GetFacebookProfile(GlobalContext.AccessToken, _sCallback);
        }
       
        public static void GetFacebookFriendsFromLinkUser(App42ApiResultCallback callBack)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(callBack);
                if (mSocialService == null)
                {
                    mSocialService = GlobalContext.SERVICE_API.BuildSocialService();
                }
                mSocialService.GetFacebookFriendsFromAccessToken(GlobalContext.AccessToken, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void GetTopNFacebookFriendsScores(int count, App42ApiResultCallback callBack)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(callBack);
                if (mScoreBoardService == null)
                {
                    mScoreBoardService = GlobalContext.SERVICE_API.BuildScoreBoardService();
                }
                mScoreBoardService.GetTopNRankersFromFacebook(GlobalContext.gameName, GlobalContext.AccessToken, count, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void GetTopNGlobalScores(int count, App42ApiResultCallback callBack)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(callBack);
                if (mScoreBoardService == null)
                {
                    mScoreBoardService = GlobalContext.SERVICE_API.BuildScoreBoardService();
                }
                mScoreBoardService.GetTopNRankers(GlobalContext.gameName, count, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void GetUserRanking(App42ApiResultCallback callBack)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(callBack);
                if (mScoreBoardService == null)
                {
                    mScoreBoardService = GlobalContext.SERVICE_API.BuildScoreBoardService();
                }
                mScoreBoardService.GetUserRanking(GlobalContext.gameName, GlobalContext.g_UserProfile.UserID, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void GetUserHighestScore(App42ApiResultCallback callBack)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(callBack);
                if (mScoreBoardService == null)
                {
                    mScoreBoardService = GlobalContext.SERVICE_API.BuildScoreBoardService();
                }
                mScoreBoardService.GetHighestScoreByUser(GlobalContext.gameName, GlobalContext.g_UserProfile.UserID, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void SaveUserScore(double _score, App42ApiResultCallback callBack)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(callBack);
                if (mScoreBoardService == null)
                {
                    mScoreBoardService = GlobalContext.SERVICE_API.BuildScoreBoardService();
                }
                mScoreBoardService.SaveUserScore(GlobalContext.gameName, GlobalContext.g_UserProfile.UserID, _score, _sCallback);
                //mScoreBoardService.GetTopNRankersFromFacebook(GlobalContext.gameName, GlobalContext.AccessToken, 10, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void GetFacebookProfilesFromIds(IList<String> facebookIds, App42ApiResultCallback requestCallback)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(requestCallback);
                if (mSocialService == null)
                {
                    mSocialService = GlobalContext.SERVICE_API.BuildSocialService();
                }
                mSocialService.GetFacebookProfilesFromIds(facebookIds, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        public static void SendMessage(String message, App42ApiResultCallback requestCallback)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(requestCallback);
                if (mStorageService == null)
                {
                    mStorageService = GlobalContext.SERVICE_API.BuildStorageService();
                }
                mStorageService.InsertJSONDocument(GlobalContext.databaseName, GlobalContext.collectionName, message, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void GetMessages(App42ApiResultCallback requestCallback)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(requestCallback);
                if (mStorageService == null)
                {
                    mStorageService = GlobalContext.SERVICE_API.BuildStorageService();
                }
                mStorageService.FindDocumentByKeyValue(GlobalContext.databaseName, GlobalContext.collectionName, "RecepientID", GlobalContext.g_UserProfile.UserID, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void DeleteMessage(String messageID, App42ApiResultCallback requestCallback)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(requestCallback);
                if (mStorageService == null)
                {
                    mStorageService = GlobalContext.SERVICE_API.BuildStorageService();
                }
                mStorageService.DeleteDocumentById(GlobalContext.databaseName, GlobalContext.collectionName, messageID, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void ShareStatus(String message, App42ApiResultCallback requestCallback)
        {
            try
            {
                App42ApiCallback _sCallback = new App42ApiCallback(requestCallback);
                if (mSocialService == null)
                {
                    mSocialService = GlobalContext.SERVICE_API.BuildSocialService();
                }
                mSocialService.UpdateFacebookStatus(GlobalContext.g_UserProfile.UserID,message, _sCallback);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public class App42ApiCallback : App42Callback
        {
            App42ApiResultCallback mShowResultCallback = null;
            public App42ApiCallback(App42ApiResultCallback callBack)
            {
                mShowResultCallback = callBack;
            }
            public void OnException(App42Exception exception)
            {
                Deployment.Current.Dispatcher.BeginInvoke(new App42ApiResultCallback(mShowResultCallback), exception, true);
            }

            public void OnSuccess(object response)
            {
                Deployment.Current.Dispatcher.BeginInvoke(new App42ApiResultCallback(mShowResultCallback), response, false);
            }
        }
    }
}
