using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Playlist
{
    public class Playlist : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private PlaylistViewModel playlistViewModel { get; set; }

        private PlaylistView playlistView { get; set; }

        public ObservableCollection<Game> PlaylistGames { get; set; }

        private const string playlistPath = "playlist.txt";

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = "Playlist",
                Type = SiderbarItemType.View,
                Icon = new TextBlock
                {
                    Text = "\ueca6", // Circled play button
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                },
                Opened = () => { return playlistView; }
            };
        }
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = "Add to Playlist",
                Action = (itemArgs) =>
                {
                    foreach (Game game in args.Games)
                    {
                        playlistViewModel.PlaylistGames.AddMissing(game);
                    }
                }
            };
        }

        public override Guid Id { get; } = Guid.Parse("b0313f81-2b86-4eba-9f24-1a727dedbd45");

        public Playlist(IPlayniteAPI api) : base(api)
        {
            // Ensure the library loaded now, relative to the extension DLL.
            // If the XAML trys to load it later it will incorrectly load it relative to Playnite's executable
            Assembly.Load("GongSolutions.WPF.DragDrop");
        }

        private IEnumerable<Game> loadPlaylistFile()
        {
            var path = Path.Combine(GetPluginUserDataPath(), playlistPath);
            if (File.Exists(path)) {
                foreach (var guid in File.ReadLines(path))
                {
                    var game = this.PlayniteApi.Database.Games.Get(Guid.Parse(guid));
                    if (game != null)
                    {
                        yield return game;
                    }
                }
            }
        }

        private void updatePlaylistFile()
        {
            var path = Path.Combine(GetPluginUserDataPath(), playlistPath);
            File.WriteAllLines(path, this.PlaylistGames.Select((g) => g.Id.ToString()));
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Initialization is done inside OnApplicationStarted, otherwise
            // loadPlaylistFile runs too early in Playnite's startup and
            // cannot call PlayniteApi.Database.Games.Get()

            PlaylistGames = new ObservableCollection<Game>(loadPlaylistFile());
            PlaylistGames.CollectionChanged += (sender, changedArgs) =>
            {
                updatePlaylistFile();
            };
            PlayniteApi.Database.Games.ItemCollectionChanged += (sender, changedArgs) =>
            {
                foreach (Game game in changedArgs.RemovedItems)
                {
                    PlaylistGames.Remove(game);
                }
            };
            playlistViewModel = new PlaylistViewModel(this);
            playlistView = new PlaylistView(playlistViewModel);
        }
    }
}