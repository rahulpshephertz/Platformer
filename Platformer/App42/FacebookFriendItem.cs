using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Platformer.App42
{
   public class FacebookFriendItem:INotifyPropertyChanged
    {
        private string _icon = "/Images/user.png";
        public string Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                if (value != _icon)
                {
                    _icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }
        }

        private string _userid;
        public string UserId
        {
            get
            {
                return _userid;
            }
            set
            {
                if (value != _userid)
                {
                    _userid = value;
                    NotifyPropertyChanged("UserId");
                }
            }
        }

        private string _rank;
        public string Rank
        {
            get
            {
                return _rank;
            }
            set
            {
                if (value != _rank)
                {
                    _rank = value;
                    NotifyPropertyChanged("Rank");
                }
            }
        }

        private string _username;
        public string UserName
        {
            get
            {
                return _username;
            }
            set
            {
                if (value != _username)
                {
                    _username = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        private string _score;
        public string Score
        {
            get
            {
                return _score;
            }
            set
            {
                if (value != _score)
                {
                    _score = value;
                    NotifyPropertyChanged("Score");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
