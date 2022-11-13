using Common;
using System.Collections.Generic;
using System.IO;

namespace MovieInfo
{
    public class CmdCleanActressImages : IAsyncCommand
    {
        #region Constructor

        public CmdCleanActressImages(ActressesDatabase actressDatabase)
        {
            m_actressDatabase = actressDatabase;
        }

        #endregion

        #region Public Functions

        public void Execute()
        {
            Logger.WriteInfo("Performing actress image cleaning");
            string imageFolder = Utilities.GetActressImageFolder();

            HashSet<string> referencedFileSet = new HashSet<string>();
            lock (m_actressDatabase)
            {
                foreach (ActressData actress in m_actressDatabase.Actresses)
                {
                    actress.ImageFileNames = Utilities.DeleteDuplicateFiles(imageFolder, actress.ImageFileNames);
                    foreach (string fileName in actress.ImageFileNames)
                        referencedFileSet.Add(Path.Combine(imageFolder, fileName));
                }
            }

            var imeageFiles = Directory.EnumerateFiles(imageFolder);
            foreach (var imeageFile in imeageFiles)
            {
                if (referencedFileSet.Contains(imeageFile) == false)
                {
                    Logger.WriteWarning("Deleting unreferenced actress image: " + Path.GetFileName(imeageFile));
                    File.Delete(imeageFile);
                }
            }
            Logger.WriteInfo("Finished actress image cleaning");
        }

        #endregion

        #region Private Members

        private ActressesDatabase m_actressDatabase;

        #endregion
    }
}
