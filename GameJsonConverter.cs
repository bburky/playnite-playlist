using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;

namespace Playlist
{
    class GameJsonConverter : JsonConverter
    {
        private IPlayniteAPI playniteApi;

        public GameJsonConverter(IPlayniteAPI api)
        {
            playniteApi = api;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Game);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return playniteApi.Database.Games.Get(Guid.Parse(s));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Game g = (Game)value;
            writer.WriteValue(g.Id);
        }
    }
}
