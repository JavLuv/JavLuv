using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace JavLuv
{
    public class ActressBrowserViewModel : ObservableObject
    {
        #region Constructors

        public ActressBrowserViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;
            m_parent.Collection.ActressesDisplayedChanged += Collection_ActressesDisplayedChanged;
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        #endregion

        #region Public Functions

        public void NotifyAllProperties()
        {
            NotifyAllPropertiesChanged();
        }

        public void NavigateLeft()
        {
            ActressDetailViewModel current = Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return;
            Parent.Overlay = new ActressDetailViewModel(this, Actresses[GetOverlayIndex() - 1]);
        }

        public bool CanNavigateLeft()
        {
            ActressDetailViewModel current = Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return false;
            int index = GetOverlayIndex();
            if (index <= 0)
                return false;
            return true;
        }

        public void NavigateRight()
        {
            ActressDetailViewModel current = Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return;
            Parent.Overlay = new ActressDetailViewModel(this, Actresses[GetOverlayIndex() + 1]);
        }

        public bool CanNavigateRight()
        {
            ActressDetailViewModel current = Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return false;
            int index = GetOverlayIndex();
            if (index == -1)
                return false;
            if (index >= Actresses.Count - 1)
                return false;
            return true;
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public ObservableCollection<ActressBrowserItemViewModel> Actresses
        {
            get { return m_actresses; }
        }

        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                if (value != m_isEnabled)
                {
                    m_isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }

        public Visibility AdvancedOptionsVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public ObservableCollection<ActressBrowserItemViewModel> SelectedItems
        {
            get { return m_selectedItems; }
            set
            {
                if (value != m_selectedItems)
                {
                    m_selectedItems = value;
                    NotifyPropertyChanged("SelectedItems");
                }
            }
        }

        public string SelectedDescription
        {
            get { return m_selectedDescription; }
            set
            {
                if (value != m_selectedDescription)
                {
                    m_selectedDescription = value;
                    NotifyPropertyChanged("SelectedDescription");
                }
            }
        }
/*
        public System.Windows.Visibility EnableMoveRenameVisibility
        {
            get
            {
                if (Settings.Get().EnableMoveRename)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility ShowAdvancedOptionsVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility FindSubtitlesVisibility
        {
            get
            {
                if (Settings.Get().ShowAdvancedOptions && String.IsNullOrEmpty(Settings.Get().Subtitles) == false)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }
*/
        #endregion

        #region Event Handlers

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SelectedItems.Count == 0)
            {
                Parent.SelectedDescription = "";
            }
            else if (SelectedItems.Count == 1)
            {
                Parent.SelectedDescription = SelectedItems[0].ActressData.Name;
            }
            else if (SelectedItems.Count > 1)
            {
                var str = new StringBuilder();
                foreach (var item in SelectedItems)
                {
                    str.Append(item.ActressData.Name);
                    if (SelectedItems.IndexOf(item) != SelectedItems.Count - 1)
                        str.Append(", ");
                }
                Parent.SelectedDescription = str.ToString();
            }
        }

        private void Collection_ActressesDisplayedChanged(object sender, EventArgs e)
        {
            // When changing names, this event is triggered.  When that happens, we
            // need to recreate the actress details with a new overlay usng the new name.
            // We first store the actress name here to find it later.
            string prevName = String.Empty;
            if (Parent.Overlay is ActressDetailViewModel)
            {
                var vm = Parent.Overlay as ActressDetailViewModel;
                prevName = vm.ActressData.Name;
            }

            // Update actress list with new browser items
            Actresses.Clear();
            foreach (var actress in Parent.Collection.ActressesDisplayed)
                Actresses.Add(new ActressBrowserItemViewModel(this, actress));

            // If we were displaying an actress detail page, we need to open a new detail page
            // using the newly created browser item viewmodel, replacing the old one.  This keeps
            // it up to date, and allows correct navigation through the list.
            if (String.IsNullOrEmpty(prevName) == false)
            {
                foreach (var actress in Actresses)
                {
                    if (actress.ActressData.Name == prevName)
                    {
                        OpenDetailView(actress);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Commands

        #region Update Actresses Command

        private void UpdateActressesExecute()
        {
            List<ActressData> actresses = new List<ActressData>();
            foreach (var browserItem in SelectedItems)
                actresses.Add(browserItem.ActressData);
            Parent.StartScan(actresses);
        }

        private bool CanUpdateActressesExecute()
        {
            return Parent.IsScanning == false && SelectedItems.Count > 0;
        }

        public ICommand UpdateActressesCommand { get { return new RelayCommand(UpdateActressesExecute, CanUpdateActressesExecute); } }

        #endregion

        #region Merge Actresses Command

        private void MergeActressesExecute()
        {
            var actressMergeDialog = new ActressMergeView(this);
            actressMergeDialog.Owner = Application.Current.MainWindow;
            actressMergeDialog.ShowDialog();
        }

        private bool CanMergeActressesExecute()
        {
            return Parent.IsScanning == false && SelectedItems.Count == 2;
        }

        public ICommand MergeActressesCommand { get { return new RelayCommand(MergeActressesExecute, CanMergeActressesExecute); } }

        #endregion

        #region Remove From Library Command

        private void RemoveFromLibraryExecute()
        {
            List<ActressData> actresses = new List<ActressData>();
            foreach (var item in SelectedItems)
                actresses.Add(item.ActressData);
            Parent.Collection.RemoveActresses(actresses);
        }

        private bool CanRemoveFromLibraryExecute()
        {
            return Parent.IsScanning == false;
        }

        public ICommand RemoveFromLibraryCommand { get { return new RelayCommand(RemoveFromLibraryExecute, CanRemoveFromLibraryExecute); } }

        #endregion

        #endregion

        #region Public Functions

        public void OpenDetailView(ActressBrowserItemViewModel browserItem)
        {
            Parent.Overlay = new ActressDetailViewModel(this, browserItem);
        }

        #endregion

        #region Private Functions

        private int GetOverlayIndex()
        {
            ActressDetailViewModel current = Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return -1;
            string Name = current.Name;
            for (int i = 0; i < Actresses.Count; ++i)
            {
                if (Actresses[i].ActressData.Name == Name)
                    return i;
            }
            return -1;
        }

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private ObservableCollection<ActressBrowserItemViewModel> m_actresses = new ObservableCollection<ActressBrowserItemViewModel>();
        private ObservableCollection<ActressBrowserItemViewModel> m_selectedItems = new ObservableCollection<ActressBrowserItemViewModel>();
        private string m_selectedDescription = String.Empty;
        private bool m_isEnabled = true;

        #endregion
    }
}

