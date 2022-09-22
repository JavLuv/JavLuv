using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace JavLuv
{

    [Serializable]
    public class SemanticVersion : IComparable
    {
        #region Constructors

        public SemanticVersion()
        {
        }

        public SemanticVersion(string version)
        {
            string[] versions = version.Split('.');
            if (versions.Count() < 3)
                throw new ArgumentException("Version string is not correctly formatted");
            Major = int.Parse(versions[0].Substring(1));
            Minor = int.Parse(versions[1]);
            Patch = int.Parse(versions[2]);
        }

        public SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        static SemanticVersion()
        {
            var currentVersion = typeof(SettingsViewModel).Assembly.GetName().Version;
            s_current = new SemanticVersion(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
        }

        #endregion

        #region Properties

        [XmlAttribute]
        public int Major { get; set; }
        [XmlAttribute]
        public int Minor { get; set; }
        [XmlAttribute]
        public int Patch { get; set; }

        #endregion

        #region Public Functions

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(32);
            sb.Append('v');
            sb.Append(Major.ToString());
            sb.Append('.');
            sb.Append(Minor.ToString());
            sb.Append('.');
            sb.Append(Patch.ToString());
            return sb.ToString();
        }

        public static SemanticVersion Current { get { return s_current; } }

        public int CompareTo(object obj)
        {
            SemanticVersion other = obj as SemanticVersion;
            if (other == null)
                throw new ArgumentException("Type not supported");
            if (Major < other.Major)
                return -1;
            else if (Major > other.Major)
                return 1;
            else
            {
                if (Minor < other.Minor)
                    return -1;
                else if (Minor > other.Minor)
                    return 1;
                else
                {
                    if (Patch < other.Patch)
                        return -1;
                    else if (Patch > other.Patch)
                        return 1;
                }
            }
            return 0;
        }

        #endregion

        #region Private Members

        private static SemanticVersion s_current;

        #endregion
    }

}
