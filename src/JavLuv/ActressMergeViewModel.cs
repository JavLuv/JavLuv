using System;
using System.Windows.Input;
using MovieInfo;
using Common;
using System.IO;
using System.Windows.Media;
using System.Text;
using System.Xml.Linq;

namespace JavLuv
{
    public class ActressMergeItem : ObservableObject
    {
        #region Constructor

        public ActressMergeItem(ActressData actressData)
        {
            m_actressData = actressData;
            if (m_actressData.ImageFileNames.Count != 0)
            {
                string path = Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]);
                m_loadImage = new CmdLoadImage(path, ImageSize.Thumbnail);
                m_loadImage.FinishedLoading += OnImageFinishedLoading;
                CommandQueue.ShortTask().Execute(m_loadImage);
            }
        }

        #endregion

        #region Event Handlers

        private void OnImageFinishedLoading(object sender, EventArgs e)
        {
            Image = m_loadImage.Image;
            if (Image == null)
                Logger.WriteWarning("Unable to load image " + Path.Combine(Utilities.GetActressImageFolder(), m_actressData.ImageFileNames[m_actressData.ImageIndex]));
            m_loadImage = null;
        }

        #endregion

        #region Events

        public EventHandler IsCheckedChanged;

        #endregion

        #region Properties

        public bool IsChecked
        {
            get { return m_isChecked; }
            set
            {
                if (value != m_isChecked)
                {
                    m_isChecked = value;
                    NotifyPropertyChanged("IsChecked");
                    IsCheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ImageSource Image
        {
            get
            {
                return m_image;
            }
            private set
            {
                if (value != m_image)
                {
                    m_image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        public string Information
        {
            get
            {
                StringBuilder sb = new StringBuilder(200);
                var displayName = MovieUtils.GetDisplayActressName(m_actressData.Name, Settings.Get().UseJapaneseNameOrder);
                var displayAltNames = MovieUtils.GetDisplayActressNames(m_actressData.AltNames, Settings.Get().UseJapaneseNameOrder);
                string displayAltNamesStr = Common.Utilities.StringListToString(displayAltNames);
                sb.AppendLine(String.Format("{0} {1}", TextManager.GetString("Text.MergeActressesName"), displayName));
                sb.AppendLine(String.Format("{0} {1}", TextManager.GetString("Text.ActressJapaneseName"), m_actressData.JapaneseName));
                sb.AppendLine(String.Format("{0} {1}", TextManager.GetString("Text.ActressAlternateNames"), displayAltNamesStr));
                return sb.ToString();
            }
        }

        #endregion

        #region Private Members

        private ActressData m_actressData;
        private bool m_isChecked;
        private ImageSource m_image;
        private CmdLoadImage m_loadImage;

        #endregion
    }

    public class ActressMergeViewModel : ObservableObject
    {
        #region Constructor

        public ActressMergeViewModel(ActressBrowserViewModel parent)
        {
            m_parent = parent;
            m_actressMergeItemA = new ActressMergeItem(m_parent.SelectedItems[0].ActressData);
            m_actressMergeItemB = new ActressMergeItem(m_parent.SelectedItems[1].ActressData);
            m_actressMergeItemA.IsCheckedChanged += OnActressAIsCheckedChanged;
            m_actressMergeItemB.IsCheckedChanged += OnActressBIsCheckedChanged;
        }

        #endregion

        #region Event Handlers

        private void OnActressAIsCheckedChanged(object sender, EventArgs e)
        {
            if (m_actressMergeItemA.IsChecked)
                m_actressMergeItemB.IsChecked = false;
        }

        private void OnActressBIsCheckedChanged(object sender, EventArgs e)
        {
            if (m_actressMergeItemB.IsChecked)
                m_actressMergeItemA.IsChecked = false;
        }

        #endregion

        #region Properties

        public ActressMergeItem ActressA { get { return m_actressMergeItemA; } }

        public ActressMergeItem ActressB { get { return m_actressMergeItemB; } }


        #endregion

        #region Commands

        #region Merge Actresses Command

        private void MergeActressesExecute()
        {
            if (m_actressMergeItemA.IsChecked)
            {
                 MovieUtils.MergeActresses(m_parent.SelectedItems[0].ActressData, m_parent.SelectedItems[1].ActressData);
                 m_parent.Parent.Collection.RemoveActress(m_parent.SelectedItems[1].ActressData, false);
            }
            else
            {
                MovieUtils.MergeActresses(m_parent.SelectedItems[1].ActressData, m_parent.SelectedItems[0].ActressData);
                m_parent.Parent.Collection.RemoveActress(m_parent.SelectedItems[0].ActressData, false);
            }
            m_parent.Parent.Collection.UpdateActressNames();
        }

        private bool CanMergeActressesExecute()
        {
            return m_actressMergeItemA.IsChecked || m_actressMergeItemB.IsChecked;
        }

        public ICommand MergeActressesCommand { get { return new RelayCommand(MergeActressesExecute, CanMergeActressesExecute); } }

        #endregion

        #endregion

        #region Private Members

        private ActressBrowserViewModel m_parent;
        private ActressMergeItem m_actressMergeItemA;
        private ActressMergeItem m_actressMergeItemB;

        #endregion
    }
}
