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
            JapaneseNames = new HashSet<NamePair>();
            AltNames = new HashSet<NamePair>();
        }

        public static void Filter(XDocument doc)
        {
        }

        public HashSet<MovieMetadata> Movies { get; set; }
        public HashSet<ActressData> Actresses { get; set; }
        public HashSet<NamePair> JapaneseNames { get; set; }
        public HashSet<NamePair> AltNames { get; set; }
    }

}
