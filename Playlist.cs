using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Playlist
{
    public class Playlist : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private PlaylistViewModel PlaylistViewModel { get; set; }

        private PlaylistView PlaylistView { get; set; }

        public ObservableCollection<Game> PlaylistGames { get; set; }

        // Dictionary is for future-proofing only, only use the "Default" collection for now.
        public Dictionary<string, ObservableCollection<Game>> PlaylistGamesDictionary { get; set; }

        private JsonConverter jsonConverter;

        private const string playlistJsonPath = "playlist.json";
        private const string playlistTxtPath = "playlist.txt";

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
                Opened = () => { return PlaylistView; }
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
                        PlaylistViewModel.PlaylistGames.AddMissing(game);
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

            jsonConverter = new GameJsonConverter(PlayniteApi);
        }

        private ObservableCollection<Game> LoadPlaylistFile()
        {
            string path = Path.Combine(GetPluginUserDataPath(), playlistJsonPath);
            if (File.Exists(path))
            {
                PlaylistGamesDictionary = JsonConvert.DeserializeObject<Dictionary<string, ObservableCollection<Game>>>(
                    File.ReadAllText(path),
                    jsonConverter
                );
            }
            else
            {
                // Set a default value for the Dictionary, and also perform migration from old txt format
                PlaylistGamesDictionary = new Dictionary<string, ObservableCollection<Game>>
                {
                    ["Default"] = new ObservableCollection<Game>(LoadPlaylistFileTxt())
                };

                string txtPath = Path.Combine(GetPluginUserDataPath(), playlistTxtPath);
                if (File.Exists(txtPath))
                {
                    UpdatePlaylistFile();
                    File.Delete(txtPath);
                }
            }

            // Dictionary is for future-proofing only, only use the "Default" collection for now.
            ObservableCollection<Game> playlist = PlaylistGamesDictionary["Default"];

            // Remove any nulls (games may have been removed from the library)
            for (int i = playlist.Count - 1; i >= 0; i--)
            {
                if (playlist[i] == null)
                {
                    playlist.RemoveAt(i);
                }
            }

            return playlist;
        }
        private IEnumerable<Game> LoadPlaylistFileTxt()
        {
            // Load old txt format playlist
            string path = Path.Combine(GetPluginUserDataPath(), playlistTxtPath);
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
            string path = Path.Combine(GetPluginUserDataPath(), playlistJsonPath);
            string json = JsonConvert.SerializeObject(PlaylistGamesDictionary, Formatting.Indented, jsonConverter);
            File.WriteAllText(path, json);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Initialization is done inside OnApplicationStarted, otherwise
            // LoadPlaylistFile runs too early in Playnite's startup and
            // cannot call PlayniteApi.Database.Games.Get()

            PlaylistGames = LoadPlaylistFile();
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
            PlaylistViewModel = new PlaylistViewModel(this);
            PlaylistView = new PlaylistView(PlaylistViewModel);
        }
    }
}