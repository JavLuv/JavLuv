using Common;
using MovieInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JavLuv
{
    public enum ImageSize
    {
        Full,
        Thumbnail,
    }

    public class ImageCache
    {
        #region Constructors

        static ImageCache()
        {
            s_imageCache = new ImageCache();
        }

        public ImageCache()
        {
            m_folder = Path.Combine(Utilities.GetJavLuvSettingsFolder(), "thumbnails");
            string oldfolder = Path.Combine(Utilities.GetJavLuvSettingsFolder(), "images");
            try
            {
                if (Directory.Exists(oldfolder))
                {
                    if (Directory.Exists(m_folder))
                        Directory.Delete(oldfolder, true);
                    else
                        Directory.Move(oldfolder, m_folder);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("Error moving image thumbnail folder", ex);
            }
            if (Directory.Exists(m_folder) == false)
                Directory.CreateDirectory(m_folder);
        }

        #endregion

        #region Public Functions

        public static ImageCache Get()
        {
            lock (s_imageCache)
            {
                return s_imageCache;
            }
        }

        public ImageSource Load(string filename, ImageSize imageSize = ImageSize.Full)
        {
            int resizeWidth = 0;
            int resizeHeight = 0;
            if (imageSize == ImageSize.Thumbnail)
                resizeHeight = m_resizeHeight;

            // Look for in-memory value
            ImageSource image;
            string hash = Hash(filename, resizeWidth, resizeHeight);
            if (m_imageDictionary.TryGetValue(hash, out image))
                return image;

            // Load from cache
            string cacheFileName = Path.Combine(m_folder, hash + ".png");
            if (File.Exists(cacheFileName))
            {
                using (Stream cachedImageStreamSource = File.OpenRead(cacheFileName))
                {
                    PngBitmapDecoder decoder = new PngBitmapDecoder(cachedImageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    image = decoder.Frames[0];
                    image.Freeze();
                    CacheInMemory(hash, image);
                    return image;
                }
            }

            // Check to see if the file exists.  This may not be the case after
            // a move / rename has occured.  A new command should be queued up after.
            if (File.Exists(filename) == false)
            {
                Logger.WriteWarning("Image cache: Image does not exist: " + filename);
                return null;
            }

            // Load from disk
            using (Stream imageStreamSource = File.OpenRead(filename))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = imageStreamSource;
                if (resizeWidth != 0)
                    bitmapImage.DecodePixelWidth = resizeWidth;
                if (resizeHeight != 0)
                    bitmapImage.DecodePixelHeight = resizeHeight;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                // Only cache images smaller than 400x400 pixels
                if (bitmapImage.PixelWidth < 400 && bitmapImage.PixelHeight < 400)
                {
                    CacheInMemory(hash, bitmapImage);

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    using (var fileStream = new System.IO.FileStream(cacheFileName, System.IO.FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }

                return bitmapImage;
            }
        }

        public void DeleteAll()
        {
            if (Directory.Exists(m_folder))
            {
                string[] fileNames = Directory.GetFiles(m_folder);
                foreach (string fn in fileNames)
                    Utilities.DeleteFile(fn);
            }
        }

        public void Delete(string fileName)
        {
            if (Directory.Exists(m_folder))
            {
                string hash = Hash(fileName, 0, m_resizeHeight);
                m_imageDictionary.Remove(hash);
                string cacheFileName = Path.Combine(m_folder, hash + ".png");
                Utilities.DeleteFile(cacheFileName);
            }
        }

        #endregion

        #region Private Functions

        private void CacheInMemory(string hash, ImageSource image)
        {
            m_imageDictionary.Add(hash, image);
            if (m_imageDictionary.Count > 300)
            {
                for (int i = 0; i < 50; ++i)
                    m_imageDictionary.Remove(m_imageDictionary.First().Key);
            }
        }

        private static string Hash(string filename, int resizeWidth, int resizeHeight)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(filename + resizeWidth.ToString() + resizeHeight.ToString()));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        #endregion

        #region Private Members

        private static ImageCache s_imageCache = new ImageCache();
        private const int m_resizeHeight = 250;
        private string m_folder = String.Empty;
        private Dictionary<string, ImageSource> m_imageDictionary = new Dictionary<string, ImageSource>();

        #endregion
    }
}
