using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MovieInfo
{

    public class CmdLoad : IAsyncCommand
    {
        #region Constructors

        public CmdLoad(ref CacheData cacheData, string fileName, ref BackupData backupData, string backupFilename)
        {
            m_cacheData = cacheData;
            m_cacheFilename = fileName;
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
        }

        #endregion

        #region Private Members

        private CacheData m_cacheData;
        private string m_cacheFilename;
        private BackupData m_backupData;
        private string m_backupFilename;

        #endregion
    }
}
