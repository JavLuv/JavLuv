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
            Actresses = new HashSet<ActressData>();
        }

        public static void Filter(XDocument doc)
        {
        }

        public HashSet<MovieMetadata> Movies { get; set; }
        public HashSet<ActressData> Actresses { get; set; }
        public HashSet<AltNameData> AltNames { get; set; }
    }

}
