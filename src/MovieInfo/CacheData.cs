using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MovieInfo
{
    [Serializable]
    public class CacheData
    {
        public CacheData()
        {
            Movies = new HashSet<MovieData>();
        }

        public static void Filter(XDocument doc)
        {
        }

        public HashSet<MovieData> Movies { get; set; }
    }

}
