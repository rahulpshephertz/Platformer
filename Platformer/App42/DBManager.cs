using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace Platformer
{
    public class DBManager
    {

        private static DBManager dbManager;

        internal static String DbFbToken = "FBToken";
        internal static String DB_Profile = "Other_Detail";
      
        private DBManager()
        {

        }

        public static DBManager getInstance()
        {
            if (dbManager == null)
            {
                dbManager = new DBManager();
            }
            return dbManager;
        }

        public void saveData(String dbName, Object data)
        {
            IsolatedStorageSettings.ApplicationSettings.Add(dbName, data);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
        public String getDBData(String dbName)
        {
            String data = IsolatedStorageSettings.ApplicationSettings[dbName].ToString();
            if (data != null && data.Length > 0)
            {
                return data;
            }
            else
            {
                return null;
            }
        }

        public Boolean isDBAvailable(String dbName)
        {
            try
            {
                object data = IsolatedStorageSettings.ApplicationSettings[dbName];
                return true;
            }
            catch (KeyNotFoundException knfe)
            {
                return false;
            }
            catch (Exception e)
            { 
             return false;
            }
        }

        public void cleanData(String dbName)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove(dbName);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
