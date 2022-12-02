using MovieInfo;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            if (SortList.Count >= 24)
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
    }
}
