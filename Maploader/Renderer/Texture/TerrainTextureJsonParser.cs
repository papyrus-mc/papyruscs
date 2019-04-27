using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Maploader.Renderer.Texture
{
    public class TerrainTextureJsonParser
    {
        private readonly string json;
        private readonly string path;

        public TerrainTextureJsonParser(string json, string path)
        {
            this.json = json;
            this.path = path;

            ParseJsonFile(json);
        }

        public Dictionary<string, Texture> Textures { get; } = new Dictionary<string, Texture>();

        private void ParseJsonFile(string json)
        {
            var jobj = JObject.Parse(json);
            var texturesData = jobj.SelectToken("texture_data").ToObject<JObject>();

            foreach (var textureData in texturesData.Properties())
            {
                var texture = new Texture(textureData.Name);
                Textures.Add(textureData.Name, texture);

                var textureObject = textureData.Value.ToObject<JObject>();
                var textures = textureObject["textures"];

                if (textures is JObject jo)
                {
                    HandleJOject(texture, jo);

                }
                else if (textures is JArray ja)
                {
                    foreach (var t in ja)
                    {
                        if (t is JObject ajo)
                        {
                            HandleJOject(texture, ajo);
                        }
                        else
                        {
                            HandleJToken(texture, t);
                        }
                    }
                }
                else
                {
                    HandleJToken(texture, textures);
                }
            }
        }

        private void HandleJOject(Texture texture, JObject jo)
        {
            string overlayColor = null;
            string tintColor = null;
            string path = jo["path"].Value<string>();

            if (jo.ContainsKey("overlay_color"))
                overlayColor = jo["overlay_color"].Value<string>();
            if (jo.ContainsKey("tint_color"))
                tintColor = jo["tint_color"].Value<string>();

            texture.AddSubTexture(path, overlayColor, tintColor);
        }

        private void HandleJToken(Texture texture, JToken textures)
        {
            texture.AddSubTexture(textures.Value<string>());
        }
    }
}