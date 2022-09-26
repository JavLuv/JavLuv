using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MovieInfo
{

    public class ActressStats
    {
        #region Constructors

        public ActressStats()
        {
        }

        #endregion

        #region Properties

        public int Age { get; set; }
        public bool IsBirthday { get; set; }
        public int NumMoviesInCollection { get; set; }
        public int AverageMovieRating { get; set; }

        #endregion
    }

    [Serializable]
    public class ActressData : IEquatable<ActressData>
    {
        #region Constructors

        public ActressData()
        {
            Name = String.Empty;
            JapaneseName = String.Empty;
            AlternateNames = new List<string>();
            DateOfBirth = new DateTime();
            Cup = String.Empty;
            BloodType = String.Empty;
            ImageFileNames = new List<string>();
        }

        #endregion

        #region Public Functions

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var md = obj as MovieData;
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
        public List<string> AlternateNames { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Height { get; set; }
        public string Cup { get; set; }
        public int Bust { get; set; }
        public int Waist { get; set; }
        public int Hips { get; set; }
        public string BloodType { get; set; }
        public int UserRating { get; set; }
         public int NumMovies { get; set; }
       public string Notes { get; set; }
        public List<string> ImageFileNames { get; set; }
        public int ImageIndex { get; set; }

        #endregion
    }

    [Serializable]
    public class ActressesData
    {
        #region Constructors

        public ActressesData()
        {
            Actresses = new HashSet<ActressData>();
        }

        public static void Filter(XDocument doc)
        {
        }

        #endregion

        #region Properties

        public HashSet<ActressData> Actresses { get; set; }

        #endregion
    }

}
