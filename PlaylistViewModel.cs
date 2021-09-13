using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playlist
{
    public class PlaylistViewModel : ObservableObject
    {
        private Playlist playlist;
        private IPlayniteAPI playniteApi;
        public ObservableCollection<Game> PlaylistGames { get; set; }
        public RelayCommand<object> NavigateBackCommand { get; }
        public RelayCommand<Game> StartGameCommand { get; }
        public RelayCommand<ObservableCollection<object>> RemoveGamesCommand { get; }

        public PlaylistViewModel(Playlist playlist, IPlayniteAPI playniteApi)
        {
            this.playlist = playlist;
            this.PlaylistGames = new ObservableCollection<Game>();
            this.playniteApi = playniteApi;

            NavigateBackCommand = new RelayCommand<object>((a) =>
            {
                playniteApi.MainView.SwitchToLibraryView();
            });

            StartGameCommand = new RelayCommand<Game>((game) =>
            {
                playniteApi.StartGame(game.Id);
            });

            RemoveGamesCommand = new RelayCommand<ObservableCollection<object>>((games) =>
            {
                
                foreach (var game in games.ToList())
                {
                    this.PlaylistGames.Remove(game as Game);
                }
            });
        }
    }

}
