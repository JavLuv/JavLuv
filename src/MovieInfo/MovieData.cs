using Common;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MovieInfo
{

    [Serializable]
    public class UniqueID
    {
        #region Constructors

        public UniqueID()
        {
            Type = "JP DVD-ID";
            Value = "";
            Default = true;
        }

        public UniqueID(string id) : this()
        {
            Value = id;
        }

        #endregion

        #region Properties

        [XmlAttribute("type")]
        public string Type
        { get; set; }

        [XmlAttribute("default")]
        public bool Default
        { get; set; }

        [XmlText]
        public string Value
        { get; set; }

        #endregion
    }

    [Serializable]
    [XmlRoot("rating")]
    public class RatingData
    {
        #region Properties

        [XmlAttribute("name")]
        string Name { get; set; }

        [XmlAttribute("max")]
        int Max { get; set; }

        [XmlAttribute("default")]
        bool Default { get; set; }

        [XmlAttribute("value")]
        int Value { get; set; }

        [XmlAttribute("votes")]
        int Votes { get; set; }

        #endregion
    }

    [Serializable]
    public class ActorData
    {
        #region Constructors

        public ActorData()
        {
            Aliases = new List<string>();
            Name = "";
        }
        public ActorData(string name)
        {
            Aliases = new List<string>();
            Name = name;
        }

        #endregion

        #region Properties

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("altname")]
        public List<string> Aliases { get; set; }

        [XmlElement("thumb")]
        public string Thumbnail { get; set; }

        [XmlElement("role")]
        public string Role { get; set; }

        [XmlElement("order")]
        public int Order { get; set; }

        #endregion
    }

    [Serializable]
    [XmlRoot("movie")]
    public class MovieMetadata : IEquatable<MovieMetadata>
    {
        #region Constructors

        public MovieMetadata()
        {
            UniqueID = new UniqueID();
            Title = String.Empty;
            OriginalTitle = String.Empty;
            Premiered = String.Empty;
            Director = String.Empty;
            Studio = String.Empty;
            Label = String.Empty;
            Series = String.Empty;
            Genres = new List<string>();
            Actors = new List<ActorData>();
            Rating = 0.0f;
            Ratings = new List<RatingData>();
            Plot = String.Empty;
            Status = String.Empty;
            DateAdded = String.Empty;
        }

        public MovieMetadata(string uniqueID) : this()
        {
            UniqueID = new UniqueID(uniqueID);
        }

        #endregion

        #region Public Functions

        public override int GetHashCode()
        {
            return UniqueID.Type.GetHashCode() ^ UniqueID.Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var md = obj as MovieMetadata;
            if (md == null)
                return false;
            return Equals(md);
        }

        public bool Equals(MovieMetadata other)
        {
            return UniqueID.Type == other.UniqueID.Type && UniqueID.Value == other.UniqueID.Value;
        }

        #endregion

        #region Properties

        // Reserved (used by Kodi, although Javinizer uses this field)
        [XmlElement("id")]
        public string ID { get; set; }

        [XmlElement("uniqueid")]
        public UniqueID UniqueID { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("originaltitle")]
        public string OriginalTitle { get; set; }

        [XmlElement("premiered")]
        public string Premiered { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }

        [XmlElement("director")]
        public string Director { get; set; }

        [XmlElement("studio")]
        public string Studio { get; set; }

        [XmlElement("label")]
        public string Label { get; set; }

        [XmlElement("series")]
        public string Series { get; set; }

        // Javinizer-specific
        [XmlElement("rating")]
        public float Rating { get; set; }

        [XmlElement("ratings")]
        public List<RatingData> Ratings { get; set; }

        [XmlElement("votes")]
        public string Votes { get; set; }

        [XmlElement("userrating")]
        public int UserRating { get; set; }

        [XmlElement("plot")]
        public string Plot { get; set; }

        [XmlElement("runtime")]
        public int Runtime { get; set; }

        [XmlElement("trailer")]
        public string Trailer { get; set; }

        [XmlElement("mpaa")]
        public string MPAA { get; set; }

        [XmlElement("tagline")]
        public string Tagline { get; set; }

        [XmlElement("set")]
        public string Set { get; set; }

        // Technically not compatible with Kodi spec.  Used for Javinizer compatibility
        [XmlElement("thumb")]
        public string Thumb { get; set; }

        [XmlElement("genre")]
        public List<string> Genres { get; set; }

        [XmlElement("actor")]
        public List<ActorData> Actors { get; set; }

        // Unused by Kodi but part of the spec.  Can put some extra data here
        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("dateadded")]
        public string DateAdded { get; set; }


        #endregion
    }

    [Serializable]
    public class MovieData : IEquatable<MovieData>
    {
        #region Constrctors

        public MovieData()
        {
            Metadata = new MovieMetadata();
            MovieFileNames = new List<string>();
            ExtraMovieFileNames = new List<string>();
            ThumbnailsFileNames = new List<string>();
            SubtitleFileNames = new List<string>();
        }

        public MovieData(MovieData movieData)
        {
            Metadata = movieData.Metadata;
            MovieFileNames = new List<string>();
            ExtraMovieFileNames = new List<string>();
            ThumbnailsFileNames = new List<string>();
            SubtitleFileNames = new List<string>();
            MovieFileNames.AddRange(movieData.MovieFileNames);
            ExtraMovieFileNames.AddRange(movieData.ExtraMovieFileNames);
            ThumbnailsFileNames.AddRange(movieData.ThumbnailsFileNames);
            MovieResolution = movieData.MovieResolution;
            Path = movieData.Path;
            SharedPath = movieData.SharedPath;
            Folder = movieData.Folder;
            CoverFileName = movieData.CoverFileName;
            MetadataFileName = movieData.MetadataFileName;
            SubtitleFileNames.AddRange(movieData.SubtitleFileNames);
        }

        public MovieData(string uniqueID) : this()
        {
            Metadata = new MovieMetadata(uniqueID);
        }

        #endregion

        #region Public Functions

        public static void Filter(XElement element)
        {
        }

        public static void Filter(XDocument doc)
        {
            var root = doc.Root;
            foreach (var element in root.Elements())
                Filter(element);
        }

        public override int GetHashCode()
        {
            return Metadata.UniqueID.Type.GetHashCode() ^ Metadata.UniqueID.Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var md = obj as MovieData;
            if (md == null)
                return false;
            return Equals(md);
        }

        public bool Equals(MovieData other)
        {
            return Metadata.UniqueID.Type == other.Metadata.UniqueID.Type && Metadata.UniqueID.Value == other.Metadata.UniqueID.Value;
        }

        #endregion

        #region Properties

        public string Path { get; set; }

        public bool SharedPath { get; set; }

        public string Folder { get; set; }

        public MovieMetadata Metadata { get; set; }

        public List<string> MovieFileNames { get; set; }

        public string MovieResolution { get; set; }

        public List<string> ExtraMovieFileNames { get; set; }

        public string CoverFileName { get; set; }

        public List<string> ThumbnailsFileNames { get; set; }

        public string MetadataFileName { get; set; }

        public List<string> SubtitleFileNames { get; set; }

        public bool MetadataChanged { get; set; }

        #endregion
    }
}
