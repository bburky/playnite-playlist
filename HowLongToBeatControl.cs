using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playlist
{
    public class HowLongToBeatControl : ContentControl
    {
        private static Plugin _plugin;
        private static Plugin Plugin
        {
            get
            {
                if (_plugin == null)
                {
                    _plugin = Playlist.StaticPlayniteApi.Addons.Plugins.FirstOrDefault(p => p.Id == Guid.Parse("e08cd51f-9c9a-4ee3-a094-fde03b55492f"));
                }
                return _plugin;
            }
        }

        public static bool HowLongToBeatIsInstalled => Plugin != null;

        public HowLongToBeatControl(string controlName)
        {
            if (Plugin == null)
            {
                return;
            }

            control = Plugin.GetGameViewControl(new GetGameViewControlArgs
            {
                Name = controlName,
                Mode = ApplicationMode.Desktop,
            }) as PluginUserControl;
            if (control == null)
            {
                return;
            }

            control.GameContext = DataContext as Game;
            DataContextChanged += (sender, e) =>
            {
                control.GameContext = e.NewValue as Game;
            };

            Content = control;
        }

        private PluginUserControl control;
    }

    public class HowLongToBeatProgressBar : HowLongToBeatControl
    {
        public HowLongToBeatProgressBar() : base("PluginProgressBar")
        {

        }
    }
    public class HowLongToBeatPluginButton : HowLongToBeatControl
    {
        public HowLongToBeatPluginButton() : base("PluginButton")
        {

        }
    }

}
