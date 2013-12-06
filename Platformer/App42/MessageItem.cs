using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Platformer.App42
{
    public class MessageItem : INotifyPropertyChanged
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

        private string _messageid;
        public string MessageId
        {
            get
            {
                return _messageid;
            }
            set
            {
                if (value != _messageid)
                {
                    _messageid = value;
                    NotifyPropertyChanged("MessageId");
                }
            }
        }

        private string _sendername;
        public string SenderName
        {
            get
            {
                return _sendername;
            }
            set
            {
                if (value != _sendername)
                {
                    _sendername = value;
                    _sendername = _sendername.Replace("Rajasthani", "Panchal");
                    NotifyPropertyChanged("SenderName");
                }
            }
        }

        private string _message = "Hi,How are you!!!";
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    NotifyPropertyChanged("Message");
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
