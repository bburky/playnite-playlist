using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace Playlist
{
    public class Playlist : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private PlaylistViewModel PlaylistViewModel { get; set; }

        private PlaylistView PlaylistView { get; set; }

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
                Opened = () => {
                    if (PlaylistViewModel == null)
                    {
                        PlaylistViewModel = new PlaylistViewModel(PlaylistGames, PlayniteApi);
                        PlaylistView = new PlaylistView(PlaylistViewModel);
                    }
                    return PlaylistView;
                }
            };
        }
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = "Add to Playlist",
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png"),
                Action = (itemArgs) =>
                {
                    foreach (Game game in args.Games)
                    {
                        PlaylistGames.AddMissing(game);
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

        private IEnumerable<Game> LoadPlaylistFile()
        {
            string path = Path.Combine(GetPluginUserDataPath(), playlistPath);
            if (File.Exists(path))
            {
                foreach (string guid in File.ReadLines(path))
                {
                    Game game = PlayniteApi.Database.Games.Get(Guid.Parse(guid));
                    if (game != null)
                    {
                        yield return game;
                    }
                }
            }
        }

        private void UpdatePlaylistFile()
        {
            string path = Path.Combine(GetPluginUserDataPath(), playlistPath);
            File.WriteAllLines(path, PlaylistGames.Select((g) => g.Id.ToString()));
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Initialization is done inside OnApplicationStarted, otherwise
            // loadPlaylistFile runs too early in Playnite's startup and
            // cannot call PlayniteApi.Database.Games.Get()

            PlaylistGames = new ObservableCollection<Game>(LoadPlaylistFile());
            PlaylistGames.CollectionChanged += (sender, changedArgs) =>
            {
                UpdatePlaylistFile();
            };
            PlayniteApi.Database.Games.ItemCollectionChanged += (sender, changedArgs) =>
            {
                foreach (Game game in changedArgs.RemovedItems)
                {
                    PlaylistGames.Remove(game);
                }
            };
        }
    }
}