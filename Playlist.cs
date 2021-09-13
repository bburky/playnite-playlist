using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
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
        private IPlayniteAPI playniteApi;

        private List<Game> playlistGames { get; set; }
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

            playlistGames = new List<Game>();
            playniteApi = api;
            playlistViewModel = new PlaylistViewModel(this, api);
            playlistView = new PlaylistView(playlistViewModel);
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }
    }
}