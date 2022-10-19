using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MovieInfo
{

    [Serializable]
    public class ActressData : IEquatable<ActressData>
    {
        #region Constructors

        public ActressData()
        {
            Name = String.Empty;
            JapaneseName = String.Empty;
            AltNames = new List<string>();
            Cup = String.Empty;
            BloodType = String.Empty;
            ImageFileNames = new List<string>();
            Notes = String.Empty;
        }

        public ActressData(string name) : this()
        {
            Name = name;
        }

        #endregion

        #region Public Functions

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var md = obj as ActressData;
            if (md == null)
                return false;
            return Equals(md);
        }

        public bool Equals(ActressData other)
        {
            return String.Compare(Name, other.Name, true) == 0;
        }

        #endregion

        #region Properties

        public string Name { get; set; }
        public string JapaneseName { get; set; }
        public List<string> AltNames { get; set; }
        public int DobYear { get; set; }
        public int DobMonth { get; set; }
        public int DobDay { get; set; }
        public int Height { get; set; }
        public string Cup { get; set; }
        public int Bust { get; set; }
        public int Waist { get; set; }
        public int Hips { get; set; }
        public string BloodType { get; set; }
        public int MovieCount { get; set; }
        public int UserRating { get; set; }
        public string Notes { get; set; }
        public List<string> ImageFileNames { get; set; }
        public int ImageIndex { get; set; }

        #endregion
    }

    [Serializable]
    public class NamePair : IEquatable<NamePair>
    {
        #region Constructors

        public NamePair()
        {
            AltName = String.Empty;
            Name = String.Empty;
        }
        public NamePair(string altName)
        {
            AltName = altName;
            Name = String.Empty;
        }

        public NamePair(string altName, string name)
        {
            AltName = altName;
            Name = name;
        }

        #endregion

        #region Public Functions

        public override int GetHashCode()
        {
            return AltName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var md = obj as NamePair;
            if (md == null)
                return false;
            return Equals(md);
        }

        public bool Equals(NamePair other)
        {
            return String.Compare(AltName, other.AltName, true) == 0;
        }

        #endregion

        #region Properties

        public string AltName { get; set; }

        public string Name { get; set; }

        #endregion
    }

    [Serializable]
    public class ActressesDatabase
    {
        #region Constructors

        public ActressesDatabase()
        {
            Actresses = new HashSet<ActressData>();
            JapaneseNames = new HashSet<NamePair>();
            AltNames = new HashSet<NamePair>();
        }

        public static void Filter(XDocument doc)
        {
        }

        #endregion

        #region Properties

        public HashSet<ActressData> Actresses { get; set; }

        public HashSet<NamePair> JapaneseNames { get; set; }

        public HashSet<NamePair> AltNames { get; set; }

        #endregion
    }

}
