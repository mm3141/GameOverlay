// ReSharper disable All
#pragma warning disable 1591
namespace HealthBars
{
    using System.Collections.Concurrent;
    using System.Text.Json.Serialization;

    public class SpritesheetMetadata
    {
        [JsonPropertyName("frames")]
        public ConcurrentDictionary<string, FrameObject> Frames;

        [JsonPropertyName("meta")]
        public MetaObject Meta;
    }

    public class FrameObject
    {
        [JsonPropertyName("frame")]
        public CubeObject Frame;

        [JsonPropertyName("rotated")]
        public bool Rotated;

        [JsonPropertyName("sourceSize")]
        public CubeObject SourceSize;

        [JsonPropertyName("spriteSourceSize")]
        public CubeObject SpriteSourceSize;

        [JsonPropertyName("trimmed")]
        public bool Trimmed;
    }

    public class MetaObject
    {
        [JsonPropertyName("app")]
        public string App;

        [JsonPropertyName("format")]
        public string Format;

        [JsonPropertyName("image")]
        public string Image;

        [JsonPropertyName("scale")]
        public float Scale;

        [JsonPropertyName("size")]
        public CubeObject Size;

        [JsonPropertyName("version")]
        public string Version;
    }

    public class CubeObject
    {
        [JsonPropertyName("h")]
        public float H;

        [JsonPropertyName("w")]
        public float W;
        
        [JsonPropertyName("x")]
        public float X;

        [JsonPropertyName("y")]
        public float Y;
    }
}