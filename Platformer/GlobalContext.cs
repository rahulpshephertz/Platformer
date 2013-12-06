﻿// Commenting
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using com.shephertz.app42.paas.sdk.windows;

namespace Platformer
{
    public class GlobalContext
    {
        public static String API_KEY = "Your Api key";
        public static String SECRET_KEY = "Your Secret key";
        public static ServiceAPI SERVICE_API = null;
        public static String AccessToken = null;
        public static String facebookAppId = "1412246962345063";
        public static bool isFacebookAccountLinkedToApp42 = false;
        public static bool IsQueueCreated = false;
        public static UserProfile g_UserProfile = null;
        public static String gameName = "Platformer";
        public static String databaseName = "Platformer";
        public static String collectionName = "Messages";
        public static double totalScore = 0;

    }
    public class UserProfile
    {
        public string UserID;
       // private string _name;
        public string Name;
        //{
        //    get
        //    {
        //        return _name;
        //    }
        //    set
        //    {
        //        if (value != _name)
        //        {
        //            _name = value;
        //            _name = _name.Replace("Rajasthani", "Panchal");
        //            NotifyPropertyChanged("Name");
        //        }
        //    }
        //}
        public string Picture;
        public int Score;
        public int Rank;
    }
    public class Data
    {
        public string url { get; set; }
        public bool is_silhouette { get; set; }
    }
    public class Picture
    {
        public Data data { get; set; }
    }
}
