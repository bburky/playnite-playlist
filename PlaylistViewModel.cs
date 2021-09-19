using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Playlist
{
    public class PlaylistViewModel : ObservableObject
    {
        private readonly Playlist playlist;

        private readonly IPlayniteAPI playniteApi;

        public ObservableCollection<Game> PlaylistGames { get; set; }

        public RelayCommand<object> NavigateBackCommand { get; }

        public RelayCommand<Game> StartGameCommand { get; }

        public RelayCommand<ObservableCollection<object>> RemoveGamesCommand { get; }

        public RelayCommand<ObservableCollection<object>> MoveGamesToTopCommand { get; }

        public RelayCommand<ObservableCollection<object>> MoveGamesToBottomCommand { get; }

        public RelayCommand<ObservableCollection<object>> ShowGamesInLibraryCommand { get; }

        public IEnumerable<KeyValuePair<CompletionStatus, RelayCommand<IEnumerable<object>>>> CompletionStatusCommands
        {
            get
            {
                foreach (CompletionStatus completionStatus in playniteApi.Database.CompletionStatuses.OrderBy(a => a.Name))
                {
                    yield return new KeyValuePair<CompletionStatus, RelayCommand<IEnumerable<object>>>(
                        completionStatus,
                        new RelayCommand<IEnumerable<object>>((games) =>
                        {
                            foreach (Game game in games.Cast<Game>())
                            {
                                game.CompletionStatusId = completionStatus.Id;
                                playniteApi.Database.Games.Update(game);
                            }
                        })
                    );
                }
            }
        }

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
                foreach (Game game in games.Cast<Game>().ToList())
                {
                    PlaylistGames.Remove(game);
                }
            });

            MoveGamesToTopCommand = new RelayCommand<ObservableCollection<object>>((games) =>
            {
                foreach (Game game in games.Cast<Game>().OrderBy((g) => PlaylistGames.IndexOf(g)).Reverse().ToList())
                {
                    PlaylistGames.Remove(game);
                    PlaylistGames.Insert(0, game);
                }
            });

            MoveGamesToBottomCommand = new RelayCommand<ObservableCollection<object>>((games) =>
            {
                foreach (Game game in games.Cast<Game>().OrderBy((g) => PlaylistGames.IndexOf(g)).ToList())
                {
                    PlaylistGames.Remove(game);
                    PlaylistGames.Add(game);
                }
            });

            ShowGamesInLibraryCommand = new RelayCommand<ObservableCollection<object>>((games) =>
            {
                if (games.Count == 0)
                {
                    return;
                } 
                // The Playnite API only allows selecting one game currently.
                Game game = games.Cast<Game>().First();
                // This does select the game, but does not currently scroll it into view
                playniteApi.MainView.SelectGame(game.Id);
                playniteApi.MainView.SwitchToLibraryView();
            });
        }
    }
}
