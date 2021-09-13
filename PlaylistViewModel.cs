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
        public RelayCommand<ObservableCollection<object>> MoveGamesToTopCommand { get; }
        public RelayCommand<ObservableCollection<object>> MoveGamesToBottomCommand { get; }

        public PlaylistViewModel(Playlist playlist)
        {
            this.playlist = playlist;
            PlaylistGames = playlist.PlaylistGames;
            playniteApi = playlist.PlayniteApi;

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
                foreach (var game in games.Cast<Game>().ToList())
                {
                    PlaylistGames.Remove(game);
                }
            });

            MoveGamesToTopCommand = new RelayCommand<ObservableCollection<object>>((games) =>
            {
                foreach (var game in games.Cast<Game>().OrderBy((g) => this.PlaylistGames.IndexOf(g)).Reverse().ToList())
                {
                    PlaylistGames.Remove(game as Game);
                    PlaylistGames.Insert(0, game as Game);
                }
            });

            MoveGamesToBottomCommand = new RelayCommand<ObservableCollection<object>>((games) =>
            {
                foreach (var game in games.Cast<Game>().OrderBy((g) => this.PlaylistGames.IndexOf(g)).ToList())
                {
                    PlaylistGames.Remove(game);
                    PlaylistGames.Add(game);
                }
            });
        }
    }

}
