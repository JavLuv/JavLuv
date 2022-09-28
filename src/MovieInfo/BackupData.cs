using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MovieInfo
{

    [Serializable]
    public class BackupData
    {
        public BackupData()
        {
            Movies = new HashSet<MovieMetadata>();
        }

        public static void Filter(XDocument doc)
        {
        }

        public HashSet<MovieMetadata> Movies { get; set; }
    }

}
