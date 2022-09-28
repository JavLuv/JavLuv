using System.IO;
using System.Reflection;

namespace MovieInfo
{

    public class CmdLoad : IAsyncCommand
    {
        #region Constructors

        public CmdLoad(ref CacheData cacheData, string cacheFilename, ref ActressesDatabase actresses, string actressFilename, ref BackupData backupData, string backupFilename)
        {
            m_cacheData = cacheData;
            m_cacheFilename = cacheFilename;
            m_actresses = actresses;
            m_actressFilename = actressFilename;
            m_backupData = backupData;
            m_backupFilename = backupFilename;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            lock (m_cacheData)
            {
                var cacheData = MovieSerializer<CacheData>.Load(m_cacheFilename, CacheData.Filter);

                // Copy all public read/write properties
                PropertyInfo[] properties = typeof(CacheData).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.CanRead && property.CanWrite)
                        property.SetValue(m_cacheData, property.GetValue(cacheData));
                }
            }

            lock (m_backupData)
            {
                if (File.Exists(m_backupFilename))
                {
                    var backupData = MovieSerializer<BackupData>.Load(m_backupFilename, BackupData.Filter);

                    // Copy all public read/write properties
                    PropertyInfo[] properties = typeof(BackupData).GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.CanRead && property.CanWrite)
                            property.SetValue(m_backupData, property.GetValue(backupData));
                    }
                }
            }

            lock (m_actresses)
            {
                if (File.Exists(m_actressFilename))
                {
                    var actresses = MovieSerializer<ActressesDatabase>.Load(m_actressFilename, ActressesDatabase.Filter);

                    // Copy all public read/write properties
                    PropertyInfo[] properties = typeof(ActressesDatabase).GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.CanRead && property.CanWrite)
                            property.SetValue(m_actresses, property.GetValue(actresses));
                    }
                }
            }

        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private string m_cacheFilename;
        private ActressesDatabase m_actresses;
        private string m_actressFilename;
        private BackupData m_backupData;
        private string m_backupFilename;

        #endregion
    }
}
