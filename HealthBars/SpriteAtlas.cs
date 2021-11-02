// <copyright file="SpriteAtlas.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GameHelper;
    using Newtonsoft.Json;

    /// <summary>
    ///     A class to manipulate with sprite atlas.
    /// </summary>
    public class SpriteAtlas
    {
        private readonly Dictionary<string, Sprite> sprites = new();
        /// <summary>
        ///     Spritesheet size.
        /// </summary>
        public CubeObject SpritesheetSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpriteAtlas" /> class.
        /// </summary>
        /// <param name="filePathName">File pathname to the spritesheet file.</param>
        public SpriteAtlas(string filePathName)
        {
            this.SpritesheetFilePath = filePathName;

            var filename = this.SpritesheetFilePath.Substring(
                0,
                this.SpritesheetFilePath.IndexOf(".png", StringComparison.Ordinal)
            );
            this.MetadataFilePath = $"{filename}.json";

            this.Initialize();
        }

        /// <summary>
        ///     Texture pointer.
        /// </summary>
        public IntPtr TexturePtr { get; set; }

        /// <summary>
        ///     Spritesheet metadata file path.
        /// </summary>
        private string MetadataFilePath { get; }

        /// <summary>
        ///     Spritesheet file path.
        /// </summary>
        private string SpritesheetFilePath { get; }

        private string Metadata { get; set; }

        /// <summary>
        ///     Load the spritesheet file as texture and split into sprites.
        /// </summary>
        private void Initialize()
        {
            if (!File.Exists(this.SpritesheetFilePath))
            {
                throw new FileNotFoundException($"Missing spritesheet file with name: {this.SpritesheetFilePath}");
            }

            if (!File.Exists(this.MetadataFilePath))
            {
                throw new FileNotFoundException($"Missing spritesheet metadata file with name: {this.MetadataFilePath}");
            }

            this.LoadSpritesheetFile();
            this.LoadSpritesheetMetadataFile();
            this.SplitSpritesheet();
        }

        private void SplitSpritesheet()
        {
            var metadata = JsonConvert.DeserializeObject<SpritesheetMetadata>(this.Metadata);

            if (metadata != null)
            {
                this.SpritesheetSize = metadata.Meta.Size;

                foreach (var (frameKey, frameObject) in metadata.Frames)
                {
                    var frameName = frameKey.Substring(0, frameKey.IndexOf(".png", StringComparison.Ordinal));
                    this.sprites.Add(frameName, new Sprite(frameObject.Frame, this.SpritesheetSize));
                }
            }
        }

        private void LoadSpritesheetMetadataFile()
        {
            this.Metadata = File.ReadAllText(this.MetadataFilePath);
        }

        private void LoadSpritesheetFile()
        {
            Core.Overlay.AddOrGetImagePointer(this.SpritesheetFilePath, out var p, out var _, out var _);
            this.TexturePtr = p;
        }

        /// <summary>
        ///     Get sprite.
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetSprite(string spriteName)
        {
            var hasSprite = this.sprites.TryGetValue(spriteName, out Sprite sprite);

            return hasSprite ? sprite : null;
        }
    }
}