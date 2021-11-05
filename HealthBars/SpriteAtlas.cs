// <copyright file="SpriteAtlas.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HealthBars
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using GameHelper;
    using Newtonsoft.Json;

    /// <summary>
    ///     A class to manipulate with sprite atlas.
    /// </summary>
    public class SpriteAtlas
    {
        private readonly Dictionary<string, Sprite> sprites = new();
        private readonly Dictionary<string, Vector2> virtualBoundsMap = new()
        {
            { "ES", new(104, 7) },
            { "HP", new(104, 7) },
            { "EnemyHP", new(104, 7) },
            { "EmptyHP", new(104, 7) },
            { "MonsterBar", new(108, 9) },
            { "PlayerBars", new(108, 19) },
            { "EmptyMana", new(104, 8) },
            { "Mana", new(104, 8) },
        };

        /// <summary>
        ///     Spritesheet size.
        /// </summary>
        public CubeObject SpriteSheetSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpriteAtlas" /> class.
        /// </summary>
        /// <param name="filePathName">File pathname to the sprite sheet file.</param>
        public SpriteAtlas(string filePathName)
        {
            this.SpriteSheetFilePath = filePathName;

            var filename = this.SpriteSheetFilePath.Substring(0, this.SpriteSheetFilePath.IndexOf(".png", StringComparison.Ordinal));
            this.MetadataFilePath = $"{filename}.json";

            this.Initialize();
        }

        /// <summary>
        ///     Texture pointer.
        /// </summary>
        public IntPtr TexturePtr { get; set; }

        /// <summary>
        ///     Sprite sheet metadata file path.
        /// </summary>
        private string MetadataFilePath { get; }

        /// <summary>
        ///     Sprite sheet file path.
        /// </summary>
        private string SpriteSheetFilePath { get; }

        private string Metadata { get; set; }

        /// <summary>
        ///     Load the sprite sheet file as texture and split into sprites.
        /// </summary>
        private void Initialize()
        {
            if (!File.Exists(this.SpriteSheetFilePath))
            {
                throw new FileNotFoundException($"Missing sprite sheet file with name: {this.SpriteSheetFilePath}");
            }

            if (!File.Exists(this.MetadataFilePath))
            {
                throw new FileNotFoundException($"Missing sprite sheet metadata file with name: {this.MetadataFilePath}");
            }

            this.LoadSpriteSheetFile();
            this.LoadSpriteSheetMetadataFile();
            this.SplitSpriteSheet();
        }

        private void SplitSpriteSheet()
        {
            var metadata = JsonConvert.DeserializeObject<SpritesheetMetadata>(this.Metadata);

            if (metadata != null)
            {
                this.SpriteSheetSize = metadata.Meta.Size;

                foreach (var (frameKey, frameObject) in metadata.Frames)
                {
                    var frameName = frameKey.Substring(0, frameKey.IndexOf(".png", StringComparison.Ordinal));
                    var hasVirtualBounds = this.virtualBoundsMap.TryGetValue(frameName, out var virtualBounds);

                    this.sprites.Add(
                        frameName,
                        new Sprite(
                            frameObject.Frame,
                            this.SpriteSheetSize,
                            hasVirtualBounds ? virtualBounds : new(frameObject.Frame.W, frameObject.Frame.H)
                        )
                    );
                }
            }
        }

        private void LoadSpriteSheetMetadataFile()
        {
            this.Metadata = File.ReadAllText(this.MetadataFilePath);
        }

        private void LoadSpriteSheetFile()
        {
            Core.Overlay.AddOrGetImagePointer(this.SpriteSheetFilePath, out var p, out _, out _);
            this.TexturePtr = p;
        }

        /// <summary>
        ///     Get sprite.
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetSprite(string spriteName)
        {
            var hasSprite = this.sprites.TryGetValue(spriteName, out var sprite);

            return hasSprite ? sprite : null;
        }
    }
}