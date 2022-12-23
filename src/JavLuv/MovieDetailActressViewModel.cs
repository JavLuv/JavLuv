using MovieInfo;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JavLuv
{
    public class MovieDetailActressViewModel
    {
        #region Constructor

        public MovieDetailActressViewModel(MovieDetailViewModel parent)
        {
            Parent = parent;

            var SortList = new List<MovieDetailActressItemViewModel>();
            foreach (ActorData actor in Parent.MovieData.Metadata.Actors)
                SortList.Add(new MovieDetailActressItemViewModel(this, actor));
            int maxImageCount = 6;
            if (SortList.Count >= 18)
                maxImageCount = 0;
            else if (SortList.Count >= 15)
                maxImageCount = 3;
            SortList.Sort((first, second) => second.MovieCount.CompareTo(first.MovieCount));
            Actresses = new ObservableCollection<MovieDetailActressItemViewModel>();
            foreach (var actressViewModel in SortList)
            {
                if (actressViewModel.HasImage == true)
                    Actresses.Add(actressViewModel);
            }
            foreach (var actressViewModel in SortList)
            {
                if (actressViewModel.HasImage == false)
                    Actresses.Add(actressViewModel);
            }
            for (int i = 0; i < Actresses.Count; i++)
            {
                if (i >= maxImageCount)
                    Actresses[i].SuppressVisibility = true;
            }
        }

        #endregion

        #region Properties

        public MovieDetailViewModel Parent { get; private set; }

        public ObservableCollection<MovieDetailActressItemViewModel> Actresses { get; private set; }

        #endregion

        #region Public Functions

        public void NavigateLeft()
        {
            ActressDetailViewModel current = Parent.Parent.Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return;
            MovieDetailActressItemViewModel actress = Actresses[GetOverlayIndex() - 1];
            Parent.Parent.Parent.Overlay = new ActressDetailViewModel(actress, actress.Actress);
        }

        public bool CanNavigateLeft()
        {
            ActressDetailViewModel current = Parent.Parent.Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return false;
            int index = GetOverlayIndex();
            if (index <= 0)
                return false;
            return true;
        }

        public void NavigateRight()
        {
            ActressDetailViewModel current = Parent.Parent.Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return;
            MovieDetailActressItemViewModel actress = Actresses[GetOverlayIndex() + 1];
            Parent.Parent.Parent.Overlay = new ActressDetailViewModel(actress, actress.Actress);
        }

        public bool CanNavigateRight()
        {
            ActressDetailViewModel current = Parent.Parent.Parent.Overlay as ActressDetailViewModel;
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

        #region Private Functions

        private int GetOverlayIndex()
        {
            ActressDetailViewModel current = Parent.Parent.Parent.Overlay as ActressDetailViewModel;
            if (current == null)
                return -1;
            string Name = current.ActressData.Name;
            for (int i = 0; i < Actresses.Count; ++i)
            {
                if (Actresses[i].Actress.Name == Name)
                    return i;
            }
            return -1;
        }

        #endregion
    }
}
